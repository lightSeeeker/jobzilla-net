using System;
using System.Collections.Generic;

namespace jobzilla_net.Application.Resumes.Dtos;

public class ParsedResumeDto
{
    public ParsedMetaDto Meta { get; set; } = new();
    public ParsedPersonalInfoDto PersonalInfo { get; set; } = new();
    public string? Summary { get; set; }
    
    public List<ParsedExperienceDto> Experience { get; set; } = new();
    public List<ParsedEducationDto> Education { get; set; } = new();
    public Dictionary<string, List<string>> Skills { get; set; } = new();
    public List<ParsedProjectDto> Projects { get; set; } = new();
    public List<string> Languages { get; set; } = new();
    public List<ParsedCertificationDto> Certifications { get; set; } = new();
}

public class ParsedMetaDto
{
    public string ParserVersion { get; set; } = "2.0";
    public string SourceFormat { get; set; } = "unknown";
    public int PageCount { get; set; } = 1;
    public string Language { get; set; } = "en";
    public double ParsingConfidence { get; set; } = 1.0;
}

public class ParsedPersonalInfoDto
{
    public string? FullName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? Linkedin { get; set; }
    public string? Github { get; set; }
    public string? Portfolio { get; set; }
    public double ConfidenceScore { get; set; } = 1.0;
}

public class ParsedExperienceDto
{
    public string? Title { get; set; }
    public string? Company { get; set; }
    public string? Location { get; set; }
    public string? StartDate { get; set; } // YYYY-MM
    public string? EndDate { get; set; }   // YYYY-MM
    public bool IsCurrent { get; set; }
    public List<string> Bullets { get; set; } = new();
    public double ConfidenceScore { get; set; } = 1.0;
}

public class ParsedEducationDto
{
    public string? Institution { get; set; }
    public string? Country { get; set; }
    public string? Degree { get; set; }
    public string? Field { get; set; }
    public int? StartYear { get; set; }
    public int? EndYear { get; set; }
    public string? Gpa { get; set; }
    public double ConfidenceScore { get; set; } = 1.0;
}

public class ParsedProjectDto
{
    public string? Name { get; set; }
    public string? Client { get; set; }
    public List<string> Technologies { get; set; } = new();
    public List<string> Highlights { get; set; } = new();
    public double ConfidenceScore { get; set; } = 1.0;
}

public class ParsedCertificationDto
{
    public string? Name { get; set; }
    public string? IssuingOrganization { get; set; }
    public string? IssueDate { get; set; }
    public double ConfidenceScore { get; set; } = 1.0;
}
