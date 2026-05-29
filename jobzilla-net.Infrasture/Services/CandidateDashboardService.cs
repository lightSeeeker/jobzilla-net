using jobzilla_net.Application.Candidates;
using jobzilla_net.Application.Candidates.Dtos;
using jobzilla_net.Application.Common;
using jobzilla_net.Application.Common.Interfaces;
using jobzilla_net.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace jobzilla_net.Infrasture.Services;

public class CandidateDashboardService : ICandidateDashboardService
{
    private readonly IApplicationDbContext _context;

    public CandidateDashboardService(IApplicationDbContext context)
    {
        _context = context;
    }

    private async Task<CandidateProfile> GetOrCreateProfileAsync(string userId)
    {
        var profile = await _context.CandidateProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            profile = new CandidateProfile
            {
                UserId = userId,
                FullName = "New Candidate"
            };
            _context.CandidateProfiles.Add(profile);
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        return profile;
    }

    public async Task<CandidateDashboardOverviewDto> GetDashboardOverviewAsync(string userId)
    {
        var profile = await GetOrCreateProfileAsync(userId);

        var appliedJobsCount = await _context.JobApplications
            .CountAsync(a => a.CandidateProfileId == profile.Id);

        var savedJobsCount = await _context.SavedJobs
            .CountAsync(s => s.CandidateProfileId == profile.Id);

        var alertsCount = await _context.JobAlerts
            .CountAsync(a => a.CandidateProfileId == profile.Id);

        var recentApplications = await _context.JobApplications
            .Include(a => a.JobPost)
            .ThenInclude(j => j!.EmployerProfile)
            .Where(a => a.CandidateProfileId == profile.Id)
            .OrderByDescending(a => a.AppliedAtUtc)
            .Take(5)
            .Select(a => new AppliedJobDto
            {
                ApplicationId = a.Id,
                JobPostId = a.JobPostId,
                JobTitle = a.JobPost!.Title,
                CompanyName = a.JobPost.EmployerProfile!.CompanyName,
                CompanyLogoPath = a.JobPost.EmployerProfile.LogoPath,
                Location = a.JobPost.Location,
                Status = a.Status.ToString(),
                AppliedAtUtc = a.AppliedAtUtc
            })
            .ToListAsync();

        return new CandidateDashboardOverviewDto
        {
            AppliedJobsCount = appliedJobsCount,
            SavedJobsCount = savedJobsCount,
            AlertsCount = alertsCount,
            MessagesCount = 0, // Placeholder
            RecentApplications = recentApplications
        };
    }

    public async Task<PagedResult<AppliedJobDto>> GetAppliedJobsAsync(string userId, int page = 1, int pageSize = 10)
    {
        var profile = await GetOrCreateProfileAsync(userId);

        var query = _context.JobApplications
            .Include(a => a.JobPost)
            .ThenInclude(j => j!.EmployerProfile)
            .Where(a => a.CandidateProfileId == profile.Id)
            .OrderByDescending(a => a.AppliedAtUtc);

        var totalItems = await query.CountAsync();
        
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AppliedJobDto
            {
                ApplicationId = a.Id,
                JobPostId = a.JobPostId,
                JobTitle = a.JobPost!.Title,
                CompanyName = a.JobPost.EmployerProfile!.CompanyName,
                CompanyLogoPath = a.JobPost.EmployerProfile.LogoPath,
                Location = a.JobPost.Location,
                Status = a.Status.ToString(),
                AppliedAtUtc = a.AppliedAtUtc
            })
            .ToListAsync();

        return new PagedResult<AppliedJobDto>
        {
            Items = items,
            TotalCount = totalItems,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<SavedJobDto>> GetSavedJobsAsync(string userId, int page = 1, int pageSize = 10)
    {
        var profile = await GetOrCreateProfileAsync(userId);

        var query = _context.SavedJobs
            .Include(s => s.JobPost)
            .ThenInclude(j => j!.EmployerProfile)
            .Where(s => s.CandidateProfileId == profile.Id)
            .OrderByDescending(s => s.SavedAtUtc);

        var totalItems = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SavedJobDto
            {
                JobPostId = s.JobPostId,
                JobTitle = s.JobPost!.Title,
                CompanyName = s.JobPost.EmployerProfile!.CompanyName,
                CompanyLogoPath = s.JobPost.EmployerProfile.LogoPath,
                Location = s.JobPost.Location,
                JobType = s.JobPost.EmploymentType.ToString(),
                SavedAtUtc = s.SavedAtUtc
            })
            .ToListAsync();

        return new PagedResult<SavedJobDto>
        {
            Items = items,
            TotalCount = totalItems,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CandidateProfileDto> GetProfileAsync(string userId)
    {
        var profile = await GetOrCreateProfileAsync(userId);

        return new CandidateProfileDto
        {
            Id = profile.Id,
            FullName = profile.FullName,
            ProfessionalTitle = profile.ProfessionalTitle,
            PhoneNumber = profile.PhoneNumber,
            Location = profile.Location,
            Summary = profile.Summary,
            ProfileImagePath = profile.ProfileImagePath,
            ExperienceYears = profile.ExperienceYears,
            ExpectedSalary = profile.ExpectedSalary,
            Email = "" // Controller should fill this, or we inject UserManager
        };
    }

    public async Task<bool> UpdateProfileAsync(string userId, CandidateProfileDto profileDto)
    {
        var profile = await GetOrCreateProfileAsync(userId);

        profile.FullName = profileDto.FullName;
        profile.ProfessionalTitle = profileDto.ProfessionalTitle;
        profile.PhoneNumber = profileDto.PhoneNumber;
        profile.Location = profileDto.Location;
        profile.Summary = profileDto.Summary;
        profile.ProfileImagePath = profileDto.ProfileImagePath;
        profile.ExperienceYears = profileDto.ExperienceYears;
        profile.ExpectedSalary = profileDto.ExpectedSalary;

        await _context.SaveChangesAsync(CancellationToken.None);
        return true;
    }

    public async Task<List<CandidateResumeDto>> GetResumesAsync(string userId)
    {
        var profile = await GetOrCreateProfileAsync(userId);

        return await _context.CandidateResumes
            .Where(r => r.CandidateProfileId == profile.Id)
            .OrderByDescending(r => r.IsDefault)
            .ThenByDescending(r => r.CreatedAtUtc)
            .Select(r => new CandidateResumeDto
            {
                Id = r.Id,
                Title = r.Title,
                FilePath = r.FilePath,
                IsDefault = r.IsDefault,
                IsBuilderGenerated = r.IsBuilderGenerated,
                CreatedAtUtc = r.CreatedAtUtc
            })
            .ToListAsync();
    }

    public async Task<CandidateResumeDto?> AddResumeAsync(string userId, string title, string filePath, bool isDefault)
    {
        var profile = await GetOrCreateProfileAsync(userId);

        if (isDefault)
        {
            var existingResumes = await _context.CandidateResumes
                .Where(r => r.CandidateProfileId == profile.Id)
                .ToListAsync();

            foreach (var resume in existingResumes)
            {
                resume.IsDefault = false;
            }
        }

        var newResume = new CandidateResume
        {
            CandidateProfileId = profile.Id,
            Title = title,
            FilePath = filePath,
            IsDefault = isDefault,
            IsBuilderGenerated = false
        };

        _context.CandidateResumes.Add(newResume);
        await _context.SaveChangesAsync(CancellationToken.None);

        return new CandidateResumeDto
        {
            Id = newResume.Id,
            Title = newResume.Title,
            FilePath = newResume.FilePath,
            IsDefault = newResume.IsDefault,
            IsBuilderGenerated = newResume.IsBuilderGenerated,
            CreatedAtUtc = newResume.CreatedAtUtc
        };
    }

    public async Task<CandidateResumeDto?> CreateScratchResumeAsync(string userId, string title)
    {
        var profile = await GetOrCreateProfileAsync(userId);

        var emptyDoc = new jobzilla_net.Application.Resumes.Dtos.ResumeDocument
        {
            FullName = profile.FullName,
            ProfessionalTitle = profile.ProfessionalTitle,
            Email = "", // Email is typically from User, so we leave it empty for scratch
            Phone = profile.PhoneNumber,
            Location = profile.Location,
            Summary = profile.Summary
        };

        var newResume = new CandidateResume
        {
            CandidateProfileId = profile.Id,
            Title = title,
            FilePath = "", // Not a physical file
            IsDefault = false,
            IsBuilderGenerated = true,
            DocumentData = System.Text.Json.JsonSerializer.Serialize(emptyDoc, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase })
        };

        _context.CandidateResumes.Add(newResume);
        await _context.SaveChangesAsync(CancellationToken.None);

        return new CandidateResumeDto
        {
            Id = newResume.Id,
            Title = newResume.Title,
            FilePath = newResume.FilePath,
            IsDefault = newResume.IsDefault,
            IsBuilderGenerated = newResume.IsBuilderGenerated,
            CreatedAtUtc = newResume.CreatedAtUtc
        };
    }

    public async Task<bool> DeleteResumeAsync(string userId, int resumeId)
    {
        var profile = await GetOrCreateProfileAsync(userId);

        var resume = await _context.CandidateResumes
            .FirstOrDefaultAsync(r => r.Id == resumeId && r.CandidateProfileId == profile.Id);

        if (resume == null) return false;

        _context.CandidateResumes.Remove(resume);
        await _context.SaveChangesAsync(CancellationToken.None);
        
        return true;
    }

    public async Task<bool> SetDefaultResumeAsync(string userId, int resumeId)
    {
        var profile = await GetOrCreateProfileAsync(userId);
        
        var existingResumes = await _context.CandidateResumes
            .Where(r => r.CandidateProfileId == profile.Id)
            .ToListAsync();

        var targetResume = existingResumes.FirstOrDefault(r => r.Id == resumeId);
        if (targetResume == null) return false;

        foreach (var resume in existingResumes)
        {
            resume.IsDefault = (resume.Id == resumeId);
        }

        await _context.SaveChangesAsync(CancellationToken.None);
        return true;
    }
}
