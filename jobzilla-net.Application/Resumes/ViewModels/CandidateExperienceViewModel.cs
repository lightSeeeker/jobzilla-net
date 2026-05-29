namespace jobzilla_net.Application.Resumes.ViewModels;

public class CandidateExperienceViewModel
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrentPosition { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
}
