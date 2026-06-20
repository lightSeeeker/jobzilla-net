using jobzilla_net.Application.Admin.Dtos;
using jobzilla_net.Application.Common;

namespace jobzilla_net.Application.Admin;

public interface IAdminService
{
    // Dashboard
    Task<AdminDashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default);
    Task<List<AdminActivityDto>> GetRecentActivitiesAsync(int count = 15, CancellationToken ct = default);

    // Employers
    Task<PagedResult<AdminEmployerListDto>> GetEmployersAsync(AdminEmployerQuery q, CancellationToken ct = default);
    Task<AdminEmployerDetailDto?> GetEmployerDetailAsync(int id, CancellationToken ct = default);
    Task<bool> SetUserActiveStatusAsync(string userId, bool isActive, string adminId);
    Task<bool> DeleteEmployerAsync(int id, string adminId);

    // Candidates
    Task<PagedResult<AdminCandidateListDto>> GetCandidatesAsync(AdminCandidateQuery q, CancellationToken ct = default);
    Task<AdminCandidateDetailDto?> GetCandidateDetailAsync(int id, CancellationToken ct = default);
    Task<bool> DeleteCandidateAsync(int id, string adminId);

    // Resume Templates
    Task<PagedResult<AdminResumeTemplateDto>> GetResumeTemplatesAsync(int page, int pageSize, CancellationToken ct = default);
    Task<AdminResumeTemplateDto?> GetResumeTemplateByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateResumeTemplateAsync(AdminResumeTemplateFormDto dto, string adminId);
    Task<bool> UpdateResumeTemplateAsync(int id, AdminResumeTemplateFormDto dto, string adminId);
    Task<bool> DeleteResumeTemplateAsync(int id, string adminId);
    Task<bool> ToggleResumeTemplateStatusAsync(int id, string adminId);

    // Jobs
    Task<PagedResult<AdminJobListDto>> GetJobsAsync(AdminJobQuery q, CancellationToken ct = default);
    Task<bool> UpdateJobStatusAsync(int jobId, string newStatus, string adminId);
    Task<bool> ToggleJobFeaturedAsync(int jobId, string adminId);
    Task<bool> DeleteJobAsync(int jobId, string adminId);

    // Applications
    Task<PagedResult<AdminApplicationListDto>> GetApplicationsAsync(int page, int pageSize, CancellationToken ct = default);

    // Subscription Plans
    Task<List<AdminSubscriptionPlanDto>> GetSubscriptionPlansAsync(CancellationToken ct = default);
    Task<int> CreateSubscriptionPlanAsync(AdminSubscriptionPlanFormDto dto, string adminId);
    Task<bool> UpdateSubscriptionPlanAsync(int id, AdminSubscriptionPlanFormDto dto, string adminId);
    Task<bool> DeleteSubscriptionPlanAsync(int id, string adminId);

    // Payments
    Task<PagedResult<AdminPaymentListDto>> GetPaymentsAsync(AdminPaymentQuery q, CancellationToken ct = default);
    Task<AdminPaymentStatsDto> GetPaymentStatsAsync(CancellationToken ct = default);

    // CMS
    Task<List<AdminContentPageDto>> GetContentPagesAsync(CancellationToken ct = default);
    Task<AdminContentPageDto?> GetContentPageByKeyAsync(string key, CancellationToken ct = default);
    Task<bool> UpdateContentPageAsync(string key, string title, string html, string adminId);
    Task<PagedResult<AdminBlogListDto>> GetAllBlogsAsync(int page, int pageSize, CancellationToken ct = default);
    Task<bool> AdminDeleteBlogAsync(int blogId, string adminId);
    Task<bool> AdminToggleBlogPublishAsync(int blogId, string adminId);

    // Reports
    Task<AdminReportDto> GetReportAsync(DateTime from, DateTime to, CancellationToken ct = default);

    // Audit Log
    Task<PagedResult<AdminAuditLogDto>> GetAuditLogsAsync(int page, int pageSize, CancellationToken ct = default);
    Task LogActionAsync(string adminId, string adminName, string action, string module, string? details = null);
}
