using jobzilla_net.Application.Candidates.Dtos;
using jobzilla_net.Application.Common;

namespace jobzilla_net.Application.Candidates;

public interface ICandidateDashboardService
{
    Task<CandidateDashboardOverviewDto> GetDashboardOverviewAsync(string userId);
    
    Task<PagedResult<AppliedJobDto>> GetAppliedJobsAsync(string userId, int page = 1, int pageSize = 10);
    
    Task<PagedResult<SavedJobDto>> GetSavedJobsAsync(string userId, int page = 1, int pageSize = 10);
    
    Task<CandidateProfileDto> GetProfileAsync(string userId);
    
    Task<bool> UpdateProfileAsync(string userId, CandidateProfileDto profileDto);
    
    Task<List<CandidateResumeDto>> GetResumesAsync(string userId);
    
    Task<CandidateResumeDto?> AddResumeAsync(string userId, string title, string filePath, bool isDefault);
    
    Task<bool> DeleteResumeAsync(string userId, int resumeId);
    
    Task<bool> SetDefaultResumeAsync(string userId, int resumeId);

    Task<CandidateResumeDto?> CreateScratchResumeAsync(string userId, string title);

    Task<(bool Success, string Message)> ApplyForJobAsync(string userId, int jobId, int? resumeId, string? coverLetter);
    
    Task<bool> HasAppliedForJobAsync(string userId, int jobId);
}
