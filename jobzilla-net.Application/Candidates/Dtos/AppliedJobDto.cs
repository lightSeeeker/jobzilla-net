namespace jobzilla_net.Application.Candidates.Dtos;

public class AppliedJobDto
{
    public int ApplicationId { get; set; }
    public int JobPostId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyLogoPath { get; set; }
    public string? Location { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime AppliedAtUtc { get; set; }
}
