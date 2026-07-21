using jobzilla_net.Application.Admin.Dtos;
using jobzilla_net.Application.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace jobzilla_net.Models.AdminDash;

public class AdminDashOverviewViewModel
{
    public AdminDashboardStatsDto Stats { get; set; } = new();
    public List<AdminActivityDto> RecentActivities { get; set; } = new();
}

public class AdminEmployersViewModel
{
    public PagedResult<AdminEmployerListDto> Employers { get; set; } = PagedResult<AdminEmployerListDto>.Empty(1, 10);
    public string? Keyword { get; set; }
    public string? Status { get; set; }
}

public class AdminCandidatesViewModel
{
    public PagedResult<AdminCandidateListDto> Candidates { get; set; } = PagedResult<AdminCandidateListDto>.Empty(1, 10);
    public string? Keyword { get; set; }
    public string? Status { get; set; }
}

public class AdminJobsViewModel
{
    public PagedResult<AdminJobListDto> Jobs { get; set; } = PagedResult<AdminJobListDto>.Empty(1, 10);
    public string? Keyword { get; set; }
    public string? Status { get; set; }
    public int? CategoryId { get; set; }
    public List<SelectListItem> Categories { get; set; } = new();
}

public class AdminApplicationsViewModel
{
    public PagedResult<AdminApplicationListDto> Applications { get; set; } = PagedResult<AdminApplicationListDto>.Empty(1, 10);
}

public class AdminResumeTemplatesViewModel
{
    public PagedResult<AdminResumeTemplateDto> Templates { get; set; } = PagedResult<AdminResumeTemplateDto>.Empty(1, 10);
}

public class AdminResumeTemplateFormViewModel
{
    public AdminResumeTemplateFormDto Template { get; set; } = new();
    public bool IsEditMode { get; set; }
    public int? TemplateId { get; set; }

    public IFormFile? TemplateFile { get; set; }
    public IFormFile? PreviewImage { get; set; }

    /// <summary>Existing stored paths, preserved across an edit when no new file is uploaded.</summary>
    public string? ExistingTemplateFilePath { get; set; }
    public string? ExistingPreviewImagePath { get; set; }
}

public class AdminSubscriptionPlansViewModel
{
    public List<AdminSubscriptionPlanDto> Plans { get; set; } = new();
}

public class AdminSubscriptionPlanFormViewModel
{
    public AdminSubscriptionPlanFormDto Plan { get; set; } = new();
    public bool IsEditMode { get; set; }
    public int? PlanId { get; set; }
}

public class AdminPaymentsViewModel
{
    public PagedResult<AdminPaymentListDto> Payments { get; set; } = PagedResult<AdminPaymentListDto>.Empty(1, 10);
    public AdminPaymentStatsDto Stats { get; set; } = new();
    public string? Keyword { get; set; }
    public string? Status { get; set; }
}

public class AdminContentPagesViewModel
{
    public List<AdminContentPageDto> Pages { get; set; } = new();
}

public class AdminContentPageEditViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
}

public class AdminBlogsViewModel
{
    public PagedResult<AdminBlogListDto> Blogs { get; set; } = PagedResult<AdminBlogListDto>.Empty(1, 10);
}

public class AdminReportsViewModel
{
    public AdminReportDto Report { get; set; } = new();
    public DateTime FromDate { get; set; } = DateTime.UtcNow.AddMonths(-1);
    public DateTime ToDate { get; set; } = DateTime.UtcNow;
}

public class AdminAuditLogViewModel
{
    public PagedResult<AdminAuditLogDto> Logs { get; set; } = PagedResult<AdminAuditLogDto>.Empty(1, 10);
}

public class AdminHomePageContentFormViewModel
{
    public HomePageContentDto? Item { get; set; }
    public bool IsEditMode { get; set; }
}
