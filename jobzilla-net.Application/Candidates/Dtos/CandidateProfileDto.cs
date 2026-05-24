namespace jobzilla_net.Application.Candidates.Dtos;

public class CandidateProfileDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? ProfessionalTitle { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Location { get; set; }
    public string? Summary { get; set; }
    public string? ProfileImagePath { get; set; }
    public int? ExperienceYears { get; set; }
    public decimal? ExpectedSalary { get; set; }
    
    public string Email { get; set; } = string.Empty; // From Identity User
}
