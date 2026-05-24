namespace jobzilla_net.Application.Employers.Dtos;

public class EmployerDashboardOverviewDto
{
    public int PostedJobsCount { get; set; }
    public int TotalApplicationsCount { get; set; }
    public int MessagesCount { get; set; }
    public int ShortlistedCount { get; set; }
    
    public List<RecentJobApplicationDto> RecentApplications { get; set; } = new();
}

public class RecentJobApplicationDto
{
    public int ApplicationId { get; set; }
    public int JobPostId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string? CandidateProfessionalTitle { get; set; }
    public string? CandidateProfileImage { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime AppliedAtUtc { get; set; }
}

public class EmployerProfileDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? CompanySize { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public string? LogoPath { get; set; }
    public string? BannerPath { get; set; }
    public bool IsVerified { get; set; }
}

public class EmployerJobPostDto
{
    public int JobPostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public int ApplicationsCount { get; set; }
    public string Location { get; set; } = string.Empty;
}

public class EmployerJobApplicationDto
{
    public int ApplicationId { get; set; }
    public int JobPostId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string? CandidateProfessionalTitle { get; set; }
    public string? CandidateLocation { get; set; }
    public string? CandidateProfileImage { get; set; }
    public string? ResumePath { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime AppliedAtUtc { get; set; }
}
