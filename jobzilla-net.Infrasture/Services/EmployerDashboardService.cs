using jobzilla_net.Application.Common;
using jobzilla_net.Application.Employers;
using jobzilla_net.Application.Employers.Dtos;
using jobzilla_net.Core.Entities;
using jobzilla_net.Core.Enums;
using jobzilla_net.Infrasture.Persistence;
using Microsoft.EntityFrameworkCore;

namespace jobzilla_net.Infrasture.Services;

public class EmployerboardService : IEmployerboardService
{
    private readonly ApplicationDbContext _context;

    public EmployerboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    private async Task<EmployerProfile> GetOrCreateProfileAsync(string userId)
    {
        var profile = await _context.EmployerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
        {
            profile = new EmployerProfile
            {
                UserId = userId,
                CompanyName = "My Company",
                IsVerified = false
            };
            _context.EmployerProfiles.Add(profile);
            await _context.SaveChangesAsync();
        }
        return profile;
    }

    public async Task<EmployerboardOverviewDto> GetDashboardOverviewAsync(string userId)
    {
        var profile = await GetOrCreateProfileAsync(userId);

        var postedJobsCount = await _context.JobPosts
            .CountAsync(j => j.EmployerProfileId == profile.Id);

        var applications = await _context.JobApplications
            .Include(a => a.JobPost)
            .Include(a => a.CandidateProfile)
            .Where(a => a.JobPost!.EmployerProfileId == profile.Id)
            .ToListAsync();

        var totalApplications = applications.Count;
        var shortlistedCount = applications.Count(a => a.Status == ApplicationStatus.Shortlisted);

        // Fetch recent 5 applications
        var recentApps = applications
            .OrderByDescending(a => a.AppliedAtUtc)
            .Take(5)
            .Select(a => new RecentJobApplicationDto
            {
                ApplicationId = a.Id,
                JobPostId = a.JobPostId,
                JobTitle = a.JobPost!.Title,
                CandidateName = a.CandidateProfile!.FullName,
                CandidateProfessionalTitle = a.CandidateProfile.ProfessionalTitle,
                CandidateProfileImage = a.CandidateProfile.ProfileImagePath,
                Status = a.Status.ToString(),
                AppliedAtUtc = a.AppliedAtUtc
            })
            .ToList();

        // Message count would require chat system, mock for now
        return new EmployerboardOverviewDto
        {
            PostedJobsCount = postedJobsCount,
            TotalApplicationsCount = totalApplications,
            ShortlistedCount = shortlistedCount,
            MessagesCount = 0,
            RecentApplications = recentApps
        };
    }

    public async Task<EmployerProfileDto> GetProfileAsync(string userId)
    {
        var profile = await GetOrCreateProfileAsync(userId);
        
        return new EmployerProfileDto
        {
            CompanyName = profile.CompanyName,
            Industry = profile.Industry,
            CompanySize = profile.CompanySize,
            WebsiteUrl = profile.WebsiteUrl,
            PhoneNumber = profile.PhoneNumber,
            Email = profile.Email,
            Location = profile.Location,
            Description = profile.Description,
            LogoPath = profile.LogoPath,
            BannerPath = profile.BannerPath,
            IsVerified = profile.IsVerified
        };
    }

    public async Task<bool> UpdateProfileAsync(string userId, EmployerProfileDto profileDto)
    {
        var profile = await GetOrCreateProfileAsync(userId);

        profile.CompanyName = profileDto.CompanyName;
        profile.Industry = profileDto.Industry;
        profile.CompanySize = profileDto.CompanySize;
        profile.WebsiteUrl = profileDto.WebsiteUrl;
        profile.PhoneNumber = profileDto.PhoneNumber;
        profile.Location = profileDto.Location;
        profile.Description = profileDto.Description;
        profile.LogoPath = profileDto.LogoPath;
        profile.BannerPath = profileDto.BannerPath;

        // Verify/Keep logic
        _context.EmployerProfiles.Update(profile);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<PagedResult<EmployerJobPostDto>> GetPostedJobsAsync(string userId, int page, int pageSize)
    {
        var profile = await GetOrCreateProfileAsync(userId);

        var query = _context.JobPosts
            .Include(j => j.Applications)
            .Where(j => j.EmployerProfileId == profile.Id)
            .OrderByDescending(j => j.CreatedAtUtc);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new EmployerJobPostDto
            {
                JobPostId = j.Id,
                Title = j.Title,
                Status = j.Status.ToString(),
                CreatedAtUtc = j.CreatedAtUtc,
                ExpiresAtUtc = j.ExpiresAtUtc,
                ApplicationsCount = j.Applications.Count,
                Location = j.Location
            })
            .ToListAsync();

        return new PagedResult<EmployerJobPostDto>
        {
            Items = items,
            TotalCount = totalItems,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<EmployerJobApplicationDto>> GetApplicationsAsync(string userId, int page, int pageSize, int? jobId = null)
    {
        var profile = await GetOrCreateProfileAsync(userId);

        var query = _context.JobApplications
            .Include(a => a.JobPost)
            .Include(a => a.CandidateProfile)
            .Include(a => a.CandidateResume)
            .Where(a => a.JobPost!.EmployerProfileId == profile.Id);

        if (jobId.HasValue)
        {
            query = query.Where(a => a.JobPostId == jobId.Value);
        }

        query = query.OrderByDescending(a => a.AppliedAtUtc);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new EmployerJobApplicationDto
            {
                ApplicationId = a.Id,
                JobPostId = a.JobPostId,
                JobTitle = a.JobPost!.Title,
                CandidateName = a.CandidateProfile!.FullName,
                CandidateProfessionalTitle = a.CandidateProfile.ProfessionalTitle,
                CandidateLocation = a.CandidateProfile.Location,
                CandidateProfileImage = a.CandidateProfile.ProfileImagePath,
                ResumePath = a.CandidateResume != null ? a.CandidateResume.FilePath : null,
                Status = a.Status.ToString(),
                AppliedAtUtc = a.AppliedAtUtc
            })
            .ToListAsync();

        return new PagedResult<EmployerJobApplicationDto>
        {
            Items = items,
            TotalCount = totalItems,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> UpdateApplicationStatusAsync(string userId, int applicationId, string newStatus)
    {
        var profile = await GetOrCreateProfileAsync(userId);

        var application = await _context.JobApplications
            .Include(a => a.JobPost)
            .FirstOrDefaultAsync(a => a.Id == applicationId && a.JobPost!.EmployerProfileId == profile.Id);

        if (application == null) return false;

        if (Enum.TryParse<ApplicationStatus>(newStatus, out var parsedStatus))
        {
            application.Status = parsedStatus;
            _context.JobApplications.Update(application);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        return false;
    }

    public async Task<EmployerJobCreateUpdateDto?> GetJobForEditAsync(string userId, int jobId)
    {
        var profile = await GetOrCreateProfileAsync(userId);
        var job = await _context.JobPosts
            .FirstOrDefaultAsync(j => j.Id == jobId && j.EmployerProfileId == profile.Id);

        if (job == null) return null;

        return new EmployerJobCreateUpdateDto
        {
            JobCategoryId = job.JobCategoryId,
            Title = job.Title,
            Description = job.Description ?? string.Empty,
            Requirements = job.Requirements,
            Responsibilities = job.Responsibilities,
            Location = job.Location,
            EmploymentType = job.EmploymentType,
            MinimumSalary = job.MinimumSalary,
            MaximumSalary = job.MaximumSalary,
            ExpiresAtUtc = job.ExpiresAtUtc
        };
    }

    public async Task<int> CreateJobAsync(string userId, EmployerJobCreateUpdateDto dto)
    {
        var profile = await GetOrCreateProfileAsync(userId);

        var job = new JobPost
        {
            EmployerProfileId = profile.Id,
            JobCategoryId = dto.JobCategoryId,
            Title = dto.Title,
            Slug = dto.Title.ToLower().Replace(" ", "-") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6),
            Description = dto.Description,
            Requirements = dto.Requirements,
            Responsibilities = dto.Responsibilities,
            Location = dto.Location,
            EmploymentType = dto.EmploymentType,
            Status = JobStatus.Published, // Auto-publish for now
            MinimumSalary = dto.MinimumSalary,
            MaximumSalary = dto.MaximumSalary,
            ExpiresAtUtc = dto.ExpiresAtUtc,
            IsFeatured = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.JobPosts.Add(job);
        await _context.SaveChangesAsync(CancellationToken.None);
        return job.Id;
    }

    public async Task<int> CalculateAtsScoreAsync(string userId, int applicationId)
    {
        var profile = await GetOrCreateProfileAsync(userId);
        var application = await _context.JobApplications
            .Include(a => a.JobPost)
            .Include(a => a.CandidateProfile)
            .Include(a => a.CandidateResume)
            .FirstOrDefaultAsync(a => a.Id == applicationId && a.JobPost!.EmployerProfileId == profile.Id);

        if (application == null || application.JobPost == null)
            return 0;

        var jobText = $"{application.JobPost.Title} {application.JobPost.Description} {application.JobPost.Requirements}";
        var resumeText = application.CandidateProfile?.Summary ?? "";

        if (application.CandidateResume != null && !string.IsNullOrWhiteSpace(application.CandidateResume.DocumentData))
        {
            resumeText += " " + application.CandidateResume.DocumentData;
        }

        return jobzilla_net.Application.Common.Utils.AtsScoringUtility.CalculateScore(jobText, resumeText);
    }

    public async Task<bool> UpdateJobAsync(string userId, int jobId, EmployerJobCreateUpdateDto dto)
    {
        var profile = await GetOrCreateProfileAsync(userId);
        var job = await _context.JobPosts
            .FirstOrDefaultAsync(j => j.Id == jobId && j.EmployerProfileId == profile.Id);

        if (job == null) return false;

        job.JobCategoryId = dto.JobCategoryId;
        job.Title = dto.Title;
        job.Description = dto.Description;
        job.Requirements = dto.Requirements;
        job.Responsibilities = dto.Responsibilities;
        job.Location = dto.Location;
        job.EmploymentType = dto.EmploymentType;
        job.MinimumSalary = dto.MinimumSalary;
        job.MaximumSalary = dto.MaximumSalary;
        job.ExpiresAtUtc = dto.ExpiresAtUtc;

        _context.JobPosts.Update(job);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteJobAsync(string userId, int jobId)
    {
        var profile = await GetOrCreateProfileAsync(userId);
        var job = await _context.JobPosts
            .FirstOrDefaultAsync(j => j.Id == jobId && j.EmployerProfileId == profile.Id);

        if (job == null) return false;

        // Hard delete for simplicity as requested/default behavior unless soft delete was specified
        _context.JobPosts.Remove(job);
        return await _context.SaveChangesAsync() > 0;
    }
}
