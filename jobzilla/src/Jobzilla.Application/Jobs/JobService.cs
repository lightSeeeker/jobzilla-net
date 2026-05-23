using Jobzilla.Application.Common.Interfaces;
using Jobzilla.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Jobzilla.Application.Jobs;

public class JobService : IJobService
{
    private readonly IApplicationDbContext _dbContext;

    public JobService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<JobSummaryDto>> SearchAsync(JobSearchQuery query, CancellationToken cancellationToken = default)
    {
        var jobs = _dbContext.JobPosts
            .AsNoTracking()
            .Include(job => job.EmployerProfile)
            .Include(job => job.JobCategory)
            .Where(job => !job.IsDeleted && job.Status == JobStatus.Published);

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            jobs = jobs.Where(job => job.Title.Contains(query.Keyword) || (job.Description != null && job.Description.Contains(query.Keyword)));
        }

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            jobs = jobs.Where(job => job.Location.Contains(query.Location));
        }

        if (query.CategoryId.HasValue)
        {
            jobs = jobs.Where(job => job.JobCategoryId == query.CategoryId.Value);
        }

        if (query.EmploymentType.HasValue)
        {
            jobs = jobs.Where(job => job.EmploymentType == query.EmploymentType.Value);
        }

        return await jobs
            .OrderByDescending(job => job.IsFeatured)
            .ThenByDescending(job => job.CreatedAtUtc)
            .Select(job => new JobSummaryDto
            {
                Id = job.Id,
                Title = job.Title,
                CompanyName = job.EmployerProfile != null ? job.EmployerProfile.CompanyName : string.Empty,
                CategoryName = job.JobCategory != null ? job.JobCategory.Name : string.Empty,
                Location = job.Location,
                EmploymentType = job.EmploymentType,
                MinimumSalary = job.MinimumSalary,
                MaximumSalary = job.MaximumSalary,
                CreatedAtUtc = job.CreatedAtUtc,
                IsFeatured = job.IsFeatured
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<JobSummaryDto?> GetSummaryAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.JobPosts
            .AsNoTracking()
            .Include(job => job.EmployerProfile)
            .Include(job => job.JobCategory)
            .Where(job => !job.IsDeleted && job.Id == id)
            .Select(job => new JobSummaryDto
            {
                Id = job.Id,
                Title = job.Title,
                CompanyName = job.EmployerProfile != null ? job.EmployerProfile.CompanyName : string.Empty,
                CategoryName = job.JobCategory != null ? job.JobCategory.Name : string.Empty,
                Location = job.Location,
                EmploymentType = job.EmploymentType,
                MinimumSalary = job.MinimumSalary,
                MaximumSalary = job.MaximumSalary,
                CreatedAtUtc = job.CreatedAtUtc,
                IsFeatured = job.IsFeatured
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
