using jobzilla_net.Application.Candidates;
using jobzilla_net.Application.Candidates.Dtos;
using jobzilla_net.Application.Candidates.Queries;
using jobzilla_net.Application.Common;
using jobzilla_net.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace jobzilla_net.Infrasture.Services;

/// <summary>
/// EF Core implementation of <see cref="ICandidateService"/>.
/// All queries use AsNoTracking and project directly to DTOs —
/// entities are never returned outside this class.
/// </summary>
public sealed class CandidateService : ICandidateService
{
    private readonly IApplicationDbContext _db;

    public CandidateService(IApplicationDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<PagedResult<CandidateSummaryDto>> SearchAsync(
        CandidateSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var q = _db.CandidateProfiles
            .AsNoTracking()
            .Where(c => !c.IsDeleted);

        // ── Optional filters ─────────────────────────────────────────────────

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim().ToLower();
            q = q.Where(c =>
                c.FullName.ToLower().Contains(kw) ||
                (c.ProfessionalTitle != null && c.ProfessionalTitle.ToLower().Contains(kw)));
        }

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            var loc = query.Location.Trim().ToLower();
            q = q.Where(c => c.Location != null && c.Location.ToLower().Contains(loc));
        }

        if (query.MinExperienceYears.HasValue)
        {
            q = q.Where(c => c.ExperienceYears >= query.MinExperienceYears.Value);
        }

        if (query.MaxExpectedSalary.HasValue)
        {
            q = q.Where(c =>
                c.ExpectedSalary == null ||
                c.ExpectedSalary <= query.MaxExpectedSalary.Value);
        }

        // ── Count before paging ───────────────────────────────────────────────
        var totalCount = await q.CountAsync(cancellationToken);

        // ── Order + paginate ─────────────────────────────────────────────────
        var items = await q
            .OrderByDescending(c => c.CreatedAtUtc)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new CandidateSummaryDto
            {
                Id               = c.Id,
                FullName         = c.FullName,
                ProfessionalTitle = c.ProfessionalTitle,
                Location         = c.Location,
                ProfileImagePath = c.ProfileImagePath,
                ExperienceYears  = c.ExperienceYears,
                ExpectedSalary   = c.ExpectedSalary,
                CreatedAtUtc     = c.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<CandidateSummaryDto>
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = query.Page,
            PageSize   = query.PageSize
        };
    }

    /// <inheritdoc/>
    public async Task<CandidateSummaryDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _db.CandidateProfiles
            .AsNoTracking()
            .Where(c => c.Id == id && !c.IsDeleted)
            .Select(c => new CandidateSummaryDto
            {
                Id               = c.Id,
                FullName         = c.FullName,
                ProfessionalTitle = c.ProfessionalTitle,
                Location         = c.Location,
                ProfileImagePath = c.ProfileImagePath,
                ExperienceYears  = c.ExperienceYears,
                ExpectedSalary   = c.ExpectedSalary,
                CreatedAtUtc     = c.CreatedAtUtc,
                Summary          = c.Summary
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
