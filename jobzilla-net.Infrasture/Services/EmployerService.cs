using jobzilla_net.Application.Common;
using jobzilla_net.Application.Common.Interfaces;
using jobzilla_net.Application.Employers;
using jobzilla_net.Application.Employers.Dtos;
using jobzilla_net.Application.Employers.Queries;
using jobzilla_net.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace jobzilla_net.Infrasture.Services;

/// <summary>
/// EF Core implementation of <see cref="IEmployerService"/>.
/// All queries use AsNoTracking and project directly to DTOs —
/// entities are never returned outside this class.
/// </summary>
public sealed class EmployerService : IEmployerService
{
    private readonly IApplicationDbContext _db;

    public EmployerService(IApplicationDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<PagedResult<EmployerSummaryDto>> SearchAsync(
        EmployerSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var q = _db.EmployerProfiles
            .AsNoTracking()
            .Where(e => !e.IsDeleted);

        // ── Optional filters ─────────────────────────────────────────────────

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim().ToLower();
            q = q.Where(e => e.CompanyName.ToLower().Contains(kw));
        }

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            var loc = query.Location.Trim().ToLower();
            q = q.Where(e => e.Location != null && e.Location.ToLower().Contains(loc));
        }

        if (!string.IsNullOrWhiteSpace(query.Industry))
        {
            var ind = query.Industry.Trim().ToLower();
            q = q.Where(e => e.Industry != null && e.Industry.ToLower().Contains(ind));
        }

        if (!string.IsNullOrWhiteSpace(query.CompanySize))
        {
            var size = query.CompanySize.Trim().ToLower();
            q = q.Where(e => e.CompanySize != null && e.CompanySize.ToLower() == size);
        }

        // ── Count before paging ───────────────────────────────────────────────
        var totalCount = await q.CountAsync(cancellationToken);

        // ── Order + paginate ─────────────────────────────────────────────────
        var items = await q
            .OrderByDescending(e => e.CreatedAtUtc)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(e => new EmployerSummaryDto
            {
                Id               = e.Id,
                CompanyName      = e.CompanyName,
                Industry         = e.Industry,
                Location         = e.Location,
                LogoPath         = e.LogoPath,
                IsVerified       = e.IsVerified,
                ActiveJobCount   = e.JobPosts.Count(j => j.Status == JobStatus.Published && !j.IsDeleted),
                CreatedAtUtc     = e.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<EmployerSummaryDto>
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = query.Page,
            PageSize   = query.PageSize
        };
    }

    /// <inheritdoc/>
    public async Task<EmployerSummaryDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _db.EmployerProfiles
            .AsNoTracking()
            .Where(e => e.Id == id && !e.IsDeleted)
            .Select(e => new EmployerSummaryDto
            {
                Id               = e.Id,
                CompanyName      = e.CompanyName,
                Industry         = e.Industry,
                Location         = e.Location,
                LogoPath         = e.LogoPath,
                IsVerified       = e.IsVerified,
                ActiveJobCount   = e.JobPosts.Count(j => j.Status == JobStatus.Published && !j.IsDeleted),
                CreatedAtUtc     = e.CreatedAtUtc,
                Description      = e.Description,
                WebsiteUrl       = e.WebsiteUrl,
                BannerPath       = e.BannerPath
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
