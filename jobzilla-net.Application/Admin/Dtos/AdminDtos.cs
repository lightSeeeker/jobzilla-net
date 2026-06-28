namespace jobzilla_net.Application.Admin.Dtos;

// Dashboard DTOs
public sealed class AdminDashboardStatsDto
{
    public int TotalEmployers { get; init; }
    public int TotalCandidates { get; init; }
    public int NewUsersToday { get; init; }
    public int NewUsersThisWeek { get; init; }
    public int NewUsersThisMonth { get; init; }
    public int ActiveUsers { get; init; }
    public int BlockedUsers { get; init; }
    public int VerifiedUsers { get; init; }
    public int TotalJobsPosted { get; init; }
    public int ActiveJobs { get; init; }
    public int ExpiredJobs { get; init; }
    public int TotalResumes { get; init; }
    public int TotalResumesDownloaded { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal MonthlyRevenue { get; init; }
}

public sealed class AdminActivityDto
{
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string EntityName { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; }
}

// Employer Management DTOs
public sealed class AdminEmployerListDto
{
    public int Id { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string? ContactPerson { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? LogoPath { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public string? SubscriptionPlanName { get; init; }
    public bool IsActive { get; init; }
    public bool IsVerified { get; init; }
    public bool IsDeleted { get; init; }
    public int JobsPostedCount { get; init; }
}

public sealed class AdminEmployerDetailDto
{
    public int Id { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string? Industry { get; init; }
    public string? Location { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Email { get; init; }
    public string? WebsiteUrl { get; init; }
    public string? Description { get; init; }
    public string? LogoPath { get; init; }
    public string? BannerPath { get; init; }
    public bool IsVerified { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public List<AdminJobListDto> RecentJobs { get; init; } = new();
    public AdminSubscriptionPlanDto? CurrentSubscription { get; init; }
    public List<AdminPaymentListDto> Payments { get; init; } = new();
}

public sealed class AdminEmployerQuery
{
    public string? Keyword { get; init; }
    public string? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

// Candidate Management DTOs
public sealed class AdminCandidateListDto
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public int ProfileCompletionPercent { get; init; }
    public int ResumeCount { get; init; }
    public bool IsActive { get; init; }
    public string UserId { get; init; } = string.Empty;
    public bool IsDeleted { get; init; }
}

public sealed class AdminCandidateDetailDto
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Location { get; init; }
    public string? ProfessionalTitle { get; init; }
    public string? Summary { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public List<AdminResumeListDto> Resumes { get; init; } = new();
    public List<AdminApplicationListDto> Applications { get; init; } = new();
}

public sealed class AdminCandidateQuery
{
    public string? Keyword { get; init; }
    public string? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public sealed class AdminResumeListDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public bool IsDefault { get; init; }
}

// Resume Template DTOs
public sealed class AdminResumeTemplateDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Category { get; init; }
    public string? PreviewImagePath { get; init; }
    public bool IsActive { get; init; }
    public bool IsPremium { get; init; }
    public decimal Price { get; init; }
    public decimal? DiscountPrice { get; init; }
    public string? TemplateType { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public sealed class AdminResumeTemplateFormDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Category { get; init; }
    public string TemplateFilePath { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool IsPremium { get; init; }
    public decimal Price { get; init; }
    public decimal? DiscountPrice { get; init; }
    public string? TemplateType { get; init; }
}

// Job Management DTOs
public sealed class AdminJobListDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public string? CategoryName { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsFeatured { get; init; }
    public int ApplicationsCount { get; init; }
}

public sealed class AdminJobQuery
{
    public string? Keyword { get; init; }
    public string? Status { get; init; }
    public int? CategoryId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

// Application Management DTOs
public sealed class AdminApplicationListDto
{
    public int Id { get; init; }
    public string JobTitle { get; init; } = string.Empty;
    public string CandidateName { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime AppliedAtUtc { get; init; }
}

// Subscription Plan DTOs
public sealed class AdminSubscriptionPlanDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int DurationDays { get; init; }
    public int JobPostLimit { get; init; }
    public bool CanFeatureJobs { get; init; }
}

public sealed class AdminSubscriptionPlanFormDto
{
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int DurationDays { get; init; }
    public int JobPostLimit { get; init; }
    public bool CanFeatureJobs { get; init; }
}

// Payment DTOs
public sealed class AdminPaymentListDto
{
    public int Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string UserType { get; init; } = string.Empty;
    public string Package { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Reference { get; init; } = string.Empty;
    public DateTime TransactionDateUtc { get; init; }
    public string Status { get; init; } = string.Empty;
}

public sealed class AdminPaymentStatsDto
{
    public decimal TotalRevenue { get; init; }
    public decimal MonthlyRevenue { get; init; }
    public decimal YearlyRevenue { get; init; }
}

public sealed class AdminPaymentQuery
{
    public string? Keyword { get; init; }
    public string? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

// CMS DTOs
public sealed class AdminContentPageDto
{
    public string Key { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string HtmlContent { get; init; } = string.Empty;
    public bool IsPublished { get; init; }
}

public sealed class AdminBlogListDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? AuthorName { get; init; }
    public string? CategoryName { get; init; }
    public bool IsPublished { get; init; }
    public DateTime? PublishedAtUtc { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

// Report DTOs
public sealed class AdminReportDto
{
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
    public int NewEmployersRegistered { get; init; }
    public int NewCandidatesRegistered { get; init; }
    public int JobsPosted { get; init; }
    public int ApplicationsSubmitted { get; init; }
    public int SubscriptionsPurchased { get; init; }
    public decimal TotalRevenueEarned { get; init; }
    public int ResumesCreated { get; init; }
    public int ResumesDownloaded { get; init; }
}

// Audit Log DTOs
public sealed class AdminAuditLogDto
{
    public int Id { get; init; }
    public string AdminName { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string Module { get; init; } = string.Empty;
    public string? Details { get; init; }
    public DateTime PerformedAtUtc { get; init; }
}
