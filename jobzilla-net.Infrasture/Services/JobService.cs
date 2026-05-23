using jobzilla_net.Application.Common;
using jobzilla_net.Application.Common.Interfaces;
using jobzilla_net.Application.Jobs;
using jobzilla_net.Application.Jobs.Dtos;
using jobzilla_net.Application.Jobs.Queries;
using jobzilla_net.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace jobzilla_net.Infrasture.Services;

/// <summary>
/// EF Core implementation of <see cref="IJobService"/>.
/// All queries use AsNoTracking and project directly to DTOs — entities are
/// never returned outside this class.
/// </summary>
public sealed class JobService : IJobService
{
    private readonly IApplicationDbContext _db;

    public JobService(IApplicationDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<PagedResult<JobSummaryDto>> SearchAsync(
        JobSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var q = _db.JobPosts
            .AsNoTracking()
            .Where(j => j.Status == JobStatus.Published && !j.IsDeleted);

        // ── Optional filters ─────────────────────────────────────────────────

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim().ToLower();
            q = q.Where(j =>
                j.Title.ToLower().Contains(kw) ||
                (j.Description != null && j.Description.ToLower().Contains(kw)) ||
                (j.EmployerProfile != null && j.EmployerProfile.CompanyName.ToLower().Contains(kw)));
        }

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            var loc = query.Location.Trim().ToLower();
            q = q.Where(j => j.Location.ToLower().Contains(loc));
        }

        if (query.CategoryId.HasValue)
        {
            q = q.Where(j => j.JobCategoryId == query.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.EmploymentType) &&
            Enum.TryParse<EmploymentType>(query.EmploymentType, ignoreCase: true, out var et))
        {
            q = q.Where(j => j.EmploymentType == et);
        }

        if (query.MinSalary.HasValue)
        {
            // Job satisfies min if any of its salary bounds is at or above the threshold
            q = q.Where(j =>
                (j.MinimumSalary != null && j.MinimumSalary >= query.MinSalary.Value) ||
                (j.MaximumSalary != null && j.MaximumSalary >= query.MinSalary.Value));
        }

        if (query.MaxSalary.HasValue)
        {
            // Job satisfies max if it declares a minimum salary within budget
            q = q.Where(j =>
                j.MinimumSalary == null ||
                j.MinimumSalary <= query.MaxSalary.Value);
        }

        // ── Count before paging (single round-trip via split query avoidance) ─
        var totalCount = await q.CountAsync(cancellationToken);

        // ── Order + paginate ─────────────────────────────────────────────────
        var items = await q
            .OrderByDescending(j => j.IsFeatured)
            .ThenByDescending(j => j.CreatedAtUtc)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(j => new JobSummaryDto
            {
                Id              = j.Id,
                Title           = j.Title,
                Slug            = j.Slug,
                CompanyName     = j.EmployerProfile != null ? j.EmployerProfile.CompanyName : string.Empty,
                CompanyLogoPath = j.EmployerProfile != null ? j.EmployerProfile.LogoPath : null,
                CategoryName    = j.JobCategory != null ? j.JobCategory.Name : string.Empty,
                CategoryIconCss = j.JobCategory != null ? j.JobCategory.IconCssClass : null,
                Location        = j.Location,
                EmploymentType  = j.EmploymentType.ToString(),
                MinimumSalary   = j.MinimumSalary,
                MaximumSalary   = j.MaximumSalary,
                IsFeatured      = j.IsFeatured,
                CreatedAtUtc    = j.CreatedAtUtc,
                ExpiresAtUtc    = j.ExpiresAtUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<JobSummaryDto>
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = query.Page,
            PageSize   = query.PageSize
        };
    }

    /// <inheritdoc/>
    public async Task<JobSummaryDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _db.JobPosts
            .AsNoTracking()
            .Where(j => j.Id == id && j.Status == JobStatus.Published && !j.IsDeleted)
            .Select(j => new JobSummaryDto
            {
                Id               = j.Id,
                Title            = j.Title,
                Slug             = j.Slug,
                CompanyName      = j.EmployerProfile != null ? j.EmployerProfile.CompanyName : string.Empty,
                CompanyLogoPath  = j.EmployerProfile != null ? j.EmployerProfile.LogoPath : null,
                CategoryName     = j.JobCategory != null ? j.JobCategory.Name : string.Empty,
                CategoryIconCss  = j.JobCategory != null ? j.JobCategory.IconCssClass : null,
                Location         = j.Location,
                EmploymentType   = j.EmploymentType.ToString(),
                MinimumSalary    = j.MinimumSalary,
                MaximumSalary    = j.MaximumSalary,
                IsFeatured       = j.IsFeatured,
                CreatedAtUtc     = j.CreatedAtUtc,
                ExpiresAtUtc     = j.ExpiresAtUtc,
                Description      = j.Description,
                Requirements     = j.Requirements,
                Responsibilities = j.Responsibilities
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
