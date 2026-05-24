using jobzilla_net.Application.Common;
using jobzilla_net.Application.Employers.Dtos;

namespace jobzilla_net.Application.Employers;

public interface IEmployerDashboardService
{
    Task<EmployerDashboardOverviewDto> GetDashboardOverviewAsync(string userId);
    Task<EmployerProfileDto> GetProfileAsync(string userId);
    Task<bool> UpdateProfileAsync(string userId, EmployerProfileDto profileDto);
    Task<PagedResult<EmployerJobPostDto>> GetPostedJobsAsync(string userId, int page, int pageSize);
    Task<PagedResult<EmployerJobApplicationDto>> GetApplicationsAsync(string userId, int page, int pageSize);
    Task<bool> UpdateApplicationStatusAsync(string userId, int applicationId, string newStatus);
}
