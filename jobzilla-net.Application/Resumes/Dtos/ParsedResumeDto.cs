namespace jobzilla_net.Application.Resumes.Dtos;

public class ParsedResumeDto
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    
    public List<ParsedExperienceDto> Experiences { get; set; } = new();
    public List<ParsedEducationDto> Educations { get; set; } = new();
    public List<string> Skills { get; set; } = new();
    public List<ParsedCertificationDto> Certifications { get; set; } = new();
    public List<ParsedSocialLinkDto> SocialLinks { get; set; } = new();
}

public class ParsedExperienceDto
{
    public string? CompanyName { get; set; }
    public string? JobTitle { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
}

public class ParsedEducationDto
{
    public string? InstitutionName { get; set; }
    public string? Degree { get; set; }
    public string? FieldOfStudy { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class ParsedCertificationDto
{
    public string? Name { get; set; }
    public string? IssuingOrganization { get; set; }
    public DateTime? IssueDate { get; set; }
}

public class ParsedSocialLinkDto
{
    public string? PlatformName { get; set; }
    public string? Url { get; set; }
}
