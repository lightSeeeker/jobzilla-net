using System.Text;
using System.Text.RegularExpressions;
using jobzilla_net.Application.Resumes.Dtos;
using jobzilla_net.Application.Resumes.Interfaces;
using Microsoft.Extensions.Logging;

namespace jobzilla_net.Infrasture.Services.Resumes;

public class BasicRegexResumeParser : IResumeTextParser
{
    private readonly ILogger<BasicRegexResumeParser> _logger;

    public BasicRegexResumeParser(ILogger<BasicRegexResumeParser> logger)
    {
        _logger = logger;
    }

    public Task<ParsedResumeDto> ParseAsync(string rawText, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Parsing raw resume text using Regex.");

        var dto = new ParsedResumeDto();

        if (string.IsNullOrWhiteSpace(rawText))
            return Task.FromResult(dto);

        // ── Email ──────────────────────────────────────────────────────────────
        var emailMatch = Regex.Match(rawText, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
        if (emailMatch.Success)
            dto.Email = emailMatch.Value;

        // ── Phone ──────────────────────────────────────────────────────────────
        var phoneMatch = Regex.Match(rawText, @"(\+?\d[\d\s\-().]{7,}\d)");
        if (phoneMatch.Success)
            dto.PhoneNumber = phoneMatch.Value.Trim();

        // ── Social Links ───────────────────────────────────────────────────────
        foreach (Match m in Regex.Matches(rawText, @"(https?://)?(www\.)?(linkedin\.com|github\.com|stackoverflow\.com|twitter\.com)[^\s]+", RegexOptions.IgnoreCase))
        {
            var url = m.Value.Trim();
            var platform = url.Contains("linkedin") ? "LinkedIn"
                         : url.Contains("github") ? "GitHub"
                         : url.Contains("stackoverflow") ? "StackOverflow"
                         : "Twitter";

            if (!dto.SocialLinks.Any(l => l.Url == url))
                dto.SocialLinks.Add(new ParsedSocialLinkDto { PlatformName = platform, Url = url });
        }

        // ── Skills ─────────────────────────────────────────────────────────────
        var commonSkills = new[]
        {
            "C#", "ASP.NET", "ASP.NET Core", ".NET", "JavaScript", "TypeScript", "Python", "Java", "SQL",
            "HTML", "CSS", "React", "Angular", "Vue", "Node.js", "Azure", "AWS", "Docker", "Kubernetes",
            "Git", "REST", "GraphQL", "MongoDB", "PostgreSQL", "MySQL", "Redis", "Linux", "Agile", "Scrum",
            "Flutter", "Dart", "Swift", "Kotlin", "PHP", "Ruby", "Go", "Rust", "Blazor", "Entity Framework",
            "LINQ", "MVC", "WPF", "WCF", "SignalR", "Microservices", "CI/CD", "DevOps", "TDD", "SOLID"
        };

        foreach (var skill in commonSkills)
        {
            if (Regex.IsMatch(rawText, $@"\b{Regex.Escape(skill)}\b", RegexOptions.IgnoreCase))
                if (!dto.Skills.Contains(skill, StringComparer.OrdinalIgnoreCase))
                    dto.Skills.Add(skill);
        }

        // ── Section-Based Parsing ──────────────────────────────────────────────
        // Split on common section headers (case-insensitive)
        var sectionPattern = new Regex(
            @"(?m)^[\s]*(EXPERIENCE|WORK EXPERIENCE|EMPLOYMENT|EDUCATION|ACADEMIC|CERTIFICATION|CERTIFICATIONS|PROJECTS?|PROFILE|SUMMARY|OBJECTIVE)[\s]*:?\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        var sections = SplitIntoSections(rawText, sectionPattern);

        // Name: First non-empty, non-email, non-phone line
        var lines = rawText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var line in lines.Take(10))
        {
            if (!line.Contains('@') && !Regex.IsMatch(line, @"\d{3}") &&
                line.Length >= 4 && line.Length <= 60 &&
                !Regex.IsMatch(line, @"^(RESUME|CV|CURRICULUM|PROFILE|SUMMARY|EXPERIENCE|EDUCATION)$", RegexOptions.IgnoreCase))
            {
                dto.FullName = line.Trim();
                break;
            }
        }

        // Summary/Profile
        if (sections.TryGetValue("summary", out var summaryText) || sections.TryGetValue("profile", out summaryText) || sections.TryGetValue("objective", out summaryText))
        {
            dto.Summary = summaryText?.Trim();
        }

        // Experience
        if (sections.TryGetValue("experience", out var expText) && !string.IsNullOrWhiteSpace(expText))
        {
            dto.Experiences.AddRange(ParseExperiences(expText));
        }

        // Education
        if (sections.TryGetValue("education", out var eduText) && !string.IsNullOrWhiteSpace(eduText))
        {
            dto.Educations.AddRange(ParseEducations(eduText));
        }

        // Certifications
        if (sections.TryGetValue("certifications", out var certText) && !string.IsNullOrWhiteSpace(certText))
        {
            dto.Certifications.AddRange(ParseCertifications(certText));
        }

        return Task.FromResult(dto);
    }

    // ── Section Splitter ────────────────────────────────────────────────────────
    private static Dictionary<string, string> SplitIntoSections(string text, Regex sectionPattern)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var matches = sectionPattern.Matches(text).Cast<Match>().ToList();

        for (int i = 0; i < matches.Count; i++)
        {
            var header = matches[i].Value.Trim().ToLowerInvariant()
                .Replace("work experience", "experience")
                .Replace("employment", "experience")
                .Replace("academic", "education")
                .Replace("certification", "certifications")
                .Replace("project", "projects");

            var start = matches[i].Index + matches[i].Length;
            var end = (i + 1 < matches.Count) ? matches[i + 1].Index : text.Length;
            var content = text.Substring(start, end - start).Trim();

            if (!result.ContainsKey(header))
                result[header] = content;
        }

        return result;
    }

    // ── Experience Parser ───────────────────────────────────────────────────────
    private static List<ParsedExperienceDto> ParseExperiences(string text)
    {
        var results = new List<ParsedExperienceDto>();
        // Each "block" is separated by double newlines or a line that looks like a title/company
        var blocks = Regex.Split(text, @"\n{2,}").Where(b => !string.IsNullOrWhiteSpace(b)).ToList();

        foreach (var block in blocks)
        {
            var blockLines = block.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (blockLines.Length == 0) continue;

            var exp = new ParsedExperienceDto();

            // First line: typically "Job Title at Company" or "Company | Job Title"
            var titleLine = blockLines[0];
            var atSplit = Regex.Split(titleLine, @"\bat\b|\|", RegexOptions.IgnoreCase);
            if (atSplit.Length >= 2)
            {
                exp.JobTitle = atSplit[0].Trim();
                exp.CompanyName = atSplit[1].Trim();
            }
            else
            {
                exp.JobTitle = titleLine.Trim();
            }

            // Look for dates in any line
            var dateRange = FindDateRange(string.Join(" ", blockLines));
            exp.StartDate = dateRange.Item1;
            exp.EndDate = dateRange.Item2;

            // Rest as description
            exp.Description = string.Join(" ", blockLines.Skip(1)).Trim();

            if (!string.IsNullOrWhiteSpace(exp.JobTitle))
                results.Add(exp);
        }

        return results;
    }

    // ── Education Parser ────────────────────────────────────────────────────────
    private static List<ParsedEducationDto> ParseEducations(string text)
    {
        var results = new List<ParsedEducationDto>();
        var blocks = Regex.Split(text, @"\n{2,}").Where(b => !string.IsNullOrWhiteSpace(b)).ToList();

        foreach (var block in blocks)
        {
            var blockLines = block.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (blockLines.Length == 0) continue;

            var edu = new ParsedEducationDto();
            edu.InstitutionName = blockLines[0].Trim();

            // Degree detection
            var degreeMatch = Regex.Match(block, @"(B\.?Sc|M\.?Sc|B\.?Eng|M\.?Eng|B\.?A|M\.?A|PhD|Bachelor|Master|Doctor)[^\n,]*", RegexOptions.IgnoreCase);
            if (degreeMatch.Success)
                edu.Degree = degreeMatch.Value.Trim();

            // Field of study
            var fieldMatch = Regex.Match(block, @"(?:in|of)\s+([A-Z][a-zA-Z\s]{3,30})", RegexOptions.IgnoreCase);
            if (fieldMatch.Success)
                edu.FieldOfStudy = fieldMatch.Groups[1].Value.Trim();

            var dateRange = FindDateRange(block);
            edu.StartDate = dateRange.Item1;
            edu.EndDate = dateRange.Item2;

            if (!string.IsNullOrWhiteSpace(edu.InstitutionName))
                results.Add(edu);
        }

        return results;
    }

    // ── Certification Parser ────────────────────────────────────────────────────
    private static List<ParsedCertificationDto> ParseCertifications(string text)
    {
        var results = new List<ParsedCertificationDto>();
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var line in lines)
        {
            if (line.Length < 4) continue;

            var cert = new ParsedCertificationDto { Name = line.Trim() };

            var orgMatch = Regex.Match(line, @"[-–]\s*(.+)$");
            if (orgMatch.Success)
            {
                cert.Name = line.Substring(0, orgMatch.Index).Trim();
                cert.IssuingOrganization = orgMatch.Groups[1].Value.Trim();
            }

            var dateRange = FindDateRange(line);
            cert.IssueDate = dateRange.Item1;

            results.Add(cert);
        }

        return results;
    }

    // ── Date Extractor ──────────────────────────────────────────────────────────
    private static (DateTime?, DateTime?) FindDateRange(string text)
    {
        // Match patterns: "Jan 2020 - Dec 2023" / "2020 - 2023" / "2020 – Present"
        var pattern = new Regex(
            @"(?:(?:Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[a-z]*\.?\s+)?(\d{4})\s*[-–]\s*(?:(?:Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[a-z]*\.?\s+)?(\d{4}|Present|Current|Now)",
            RegexOptions.IgnoreCase);

        var match = pattern.Match(text);
        if (!match.Success) return (null, null);

        DateTime.TryParse($"01/01/{match.Groups[1].Value}", out var start);
        DateTime? end = null;

        var endStr = match.Groups[2].Value;
        if (!Regex.IsMatch(endStr, @"Present|Current|Now", RegexOptions.IgnoreCase))
            DateTime.TryParse($"01/01/{endStr}", out var endDate);

        return (start == default ? null : start, end);
    }
}
