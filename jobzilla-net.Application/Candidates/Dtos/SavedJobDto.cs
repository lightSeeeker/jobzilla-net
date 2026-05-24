namespace jobzilla_net.Application.Candidates.Dtos;

public class SavedJobDto
{
    public int JobPostId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyLogoPath { get; set; }
    public string? Location { get; set; }
    public string? JobType { get; set; }
    public DateTime SavedAtUtc { get; set; }
}
