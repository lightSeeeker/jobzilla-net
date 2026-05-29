using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using jobzilla_net.Application.Resumes.Dtos;
using jobzilla_net.Application.Resumes.Interfaces;
using Microsoft.Extensions.Logging;

namespace jobzilla_net.Infrasture.Services.Resumes;

public class ResumeSectionItemParser : IResumeSectionItemParser
{
    private readonly ILogger<ResumeSectionItemParser> _logger;

    private static readonly string[] CommonSkills = new[]
    {
        "C#", "ASP.NET", "ASP.NET Core", ".NET", "JavaScript", "TypeScript", "Python", "Java", "SQL",
        "HTML", "CSS", "React", "Angular", "Vue", "Node.js", "Azure", "AWS", "Docker", "Kubernetes",
        "Git", "REST", "GraphQL", "MongoDB", "PostgreSQL", "MySQL", "Redis", "Linux", "Agile", "Scrum",
        "Flutter", "Dart", "Swift", "Kotlin", "PHP", "Ruby", "Go", "Rust", "Blazor", "Entity Framework",
        "LINQ", "MVC", "WPF", "WCF", "SignalR", "Microservices", "CI/CD", "DevOps", "TDD", "SOLID",
        "C++", "C", "Bootstrap", "Tailwind", "Sass", "Webpack", "Figma", "Unit Testing", "Oracle",
        "NoSQL", "Firebase", "Google Cloud", "GCP", "OAuth", "JWT", "Jira", "Confluence"
    };

    public ResumeSectionItemParser(ILogger<ResumeSectionItemParser> logger)
    {
        _logger = logger;
    }

    public void ParseSectionInto(DetectedResumeSection section, ParsedResumeDto target)
    {
        _logger.LogInformation("Parsing section {SectionType} ({RawHeading}) into ParsedResumeDto.", section.SectionType, section.RawHeading);

        switch (section.SectionType)
        {
            case ResumeSectionType.PersonalInfo:
                ParsePersonalInfo(section, target);
                break;

            case ResumeSectionType.Summary:
                target.Summary = section.RawContent.Trim();
                break;

            case ResumeSectionType.Experience:
                ParseExperience(section, target);
                break;

            case ResumeSectionType.Education:
                ParseEducation(section, target);
                break;

            case ResumeSectionType.Skills:
                ParseSkills(section, target);
                break;

            case ResumeSectionType.Certifications:
                ParseCertifications(section, target);
                break;

            case ResumeSectionType.Projects:
                ParseProjects(section, target);
                break;

            case ResumeSectionType.SocialLinks:
                ParseSocialLinks(section, target);
                break;

            case ResumeSectionType.Unknown:
                _logger.LogWarning("Section type is Unknown: {Heading}. Storing content dynamically if needed.", section.RawHeading);
                // Graceful handling of Unknown/Custom sections:
                // We can parse generic information like email, phone, and skills from unknown sections as well!
                ParseGenericText(section.RawContent, target);
                break;
        }
    }

    private void ParsePersonalInfo(DetectedResumeSection section, ParsedResumeDto target)
    {
        // 1. Email
        var emailMatch = Regex.Match(section.RawContent, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
        if (emailMatch.Success)
            target.Email = emailMatch.Value;

        // 2. Phone
        var phoneMatch = Regex.Match(section.RawContent, @"(\+?\d[\d\s\-().]{7,}\d)");
        if (phoneMatch.Success)
            target.PhoneNumber = phoneMatch.Value.Trim();

        // 3. Social Links
        ExtractSocialLinksFromText(section.RawContent, target);

        // 4. Name Detection: First non-empty, non-email, non-phone line
        foreach (var line in section.Blocks.SelectMany(b => b.Lines))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(target.FullName) &&
                !trimmed.Contains('@') &&
                !Regex.IsMatch(trimmed, @"\d{3}") &&
                trimmed.Length >= 4 && trimmed.Length <= 60 &&
                !Regex.IsMatch(trimmed, @"^(RESUME|CV|CURRICULUM|PROFILE|SUMMARY|EXPERIENCE|EDUCATION|CONTACT)$", RegexOptions.IgnoreCase))
            {
                target.FullName = trimmed;
                break;
            }
        }
    }

    private void ParseExperience(DetectedResumeSection section, ParsedResumeDto target)
    {
        foreach (var block in section.Blocks)
        {
            if (block.Lines.Count == 0) continue;

            var blockLines = block.Lines;
            var exp = new ParsedExperienceDto();

            // First line: typically "Job Title at Company" or "Company | Job Title" or "Job Title - Company"
            var titleLine = blockLines[0];
            var atSplit = Regex.Split(titleLine, @"\bat\b|\||–|-", RegexOptions.IgnoreCase);
            if (atSplit.Length >= 2)
            {
                exp.JobTitle = atSplit[0].Trim();
                exp.CompanyName = atSplit[1].Trim();
                
                // Clean dates/garbage from company name
                exp.CompanyName = Regex.Replace(exp.CompanyName, @"\([\s\S]*\)", "").Trim();
            }
            else
            {
                exp.JobTitle = titleLine.Trim();
            }

            var dateRange = FindDateRange(string.Join(" ", blockLines));
            exp.StartDate = dateRange.Item1;
            exp.EndDate = dateRange.Item2;

            // Rest forms description
            var descLines = blockLines.Skip(1).Where(l => !IsDateRangeLine(l)).ToList();
            exp.Description = string.Join(Environment.NewLine, descLines).Trim();

            if (!string.IsNullOrWhiteSpace(exp.JobTitle))
                target.Experiences.Add(exp);
        }
    }

    private void ParseEducation(DetectedResumeSection section, ParsedResumeDto target)
    {
        foreach (var block in section.Blocks)
        {
            if (block.Lines.Count == 0) continue;

            var blockLines = block.Lines;
            var edu = new ParsedEducationDto();
            edu.InstitutionName = blockLines[0].Trim();

            // Degree detection
            var degreeMatch = Regex.Match(block.RawText, @"(B\.?Sc|M\.?Sc|B\.?Eng|M\.?Eng|B\.?A|M\.?A|PhD|Bachelor|Master|Doctor|Associate|Diploma)[^\n,]*", RegexOptions.IgnoreCase);
            if (degreeMatch.Success)
                edu.Degree = degreeMatch.Value.Trim();

            // Field of study
            var fieldMatch = Regex.Match(block.RawText, @"(?:in|of|major in)\s+([A-Z][a-zA-Z\s]{3,40})", RegexOptions.IgnoreCase);
            if (fieldMatch.Success)
                edu.FieldOfStudy = fieldMatch.Groups[1].Value.Trim();

            var dateRange = FindDateRange(block.RawText);
            edu.StartDate = dateRange.Item1;
            edu.EndDate = dateRange.Item2;

            if (!string.IsNullOrWhiteSpace(edu.InstitutionName))
                target.Educations.Add(edu);
        }
    }

    private void ParseSkills(DetectedResumeSection section, ParsedResumeDto target)
    {
        var lines = section.RawContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        var tokens = new List<string>();

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.StartsWith("|") && trimmedLine.EndsWith("|"))
            {
                // This is a Markdown table row!
                var cells = trimmedLine.Split('|')
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToList();
                
                // If it contains a category (cells[0]) and skills list (cells[1])
                if (cells.Count >= 2)
                {
                    var skillsCell = cells[1];
                    var subTokens = Regex.Split(skillsCell, @"[,;•*]")
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s));
                    tokens.AddRange(subTokens);
                }
            }
            else
            {
                // Standard plain line split
                var subTokens = Regex.Split(line, @"[,;•*|]")
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s));
                tokens.AddRange(subTokens);
            }
        }

        foreach (var token in tokens)
        {
            var cleanedSkill = Regex.Replace(token, @"^[\s\-•*]+", "").Trim();
            if (string.IsNullOrWhiteSpace(cleanedSkill)) continue;

            // Check match in CommonSkills
            var matchedCommon = CommonSkills.FirstOrDefault(cs => string.Equals(cs, cleanedSkill, StringComparison.OrdinalIgnoreCase));
            if (matchedCommon != null)
            {
                if (!target.Skills.Contains(matchedCommon, StringComparer.OrdinalIgnoreCase))
                    target.Skills.Add(matchedCommon);
            }
            else
            {
                // Add short, custom, non-numeric skill strings
                if (cleanedSkill.Length >= 2 && cleanedSkill.Length <= 30 &&
                    !Regex.IsMatch(cleanedSkill, @"\b(and|or|with|using|in|for|the|an|a)\b", RegexOptions.IgnoreCase) &&
                    !Regex.IsMatch(cleanedSkill, @"\d"))
                {
                    if (!target.Skills.Contains(cleanedSkill, StringComparer.OrdinalIgnoreCase))
                        target.Skills.Add(cleanedSkill);
                }
            }
        }
    }

    private void ParseCertifications(DetectedResumeSection section, ParsedResumeDto target)
    {
        foreach (var line in section.Blocks.SelectMany(b => b.Lines))
        {
            var trimmed = line.Trim();
            if (trimmed.Length < 4) continue;

            var cert = new ParsedCertificationDto { Name = trimmed };

            var orgMatch = Regex.Match(trimmed, @"[-–:]\s*(.+)$");
            if (orgMatch.Success)
            {
                cert.Name = trimmed.Substring(0, orgMatch.Index).Trim();
                cert.IssuingOrganization = orgMatch.Groups[1].Value.Trim();
                cert.IssuingOrganization = Regex.Replace(cert.IssuingOrganization, @"\([\s\S]*\)", "").Trim();
            }

            var dateRange = FindDateRange(trimmed);
            cert.IssueDate = dateRange.Item1;

            cert.Name = Regex.Replace(cert.Name, @"\([\s\S]*\)", "").Trim();

            if (!string.IsNullOrWhiteSpace(cert.Name))
                target.Certifications.Add(cert);
        }
    }

    private void ParseProjects(DetectedResumeSection section, ParsedResumeDto target)
    {
        foreach (var block in section.Blocks)
        {
            if (block.Lines.Count == 0) continue;

            var blockLines = block.Lines;
            var proj = new ParsedProjectDto();

            // First line: typically "Project Name" or "Project Name - Description"
            var titleLine = blockLines[0];
            var split = Regex.Split(titleLine, @"[-–:|]", RegexOptions.IgnoreCase);
            if (split.Length >= 2)
            {
                proj.Name = split[0].Trim();
            }
            else
            {
                proj.Name = titleLine.Trim();
            }

            // Extract project URL
            var urlMatch = Regex.Match(block.RawText, @"(https?://)?(www\.)?(github\.com|gitlab\.com|bitbucket\.org|behance\.net|dribbble\.com)[^\s\)\u00A0]+", RegexOptions.IgnoreCase);
            if (urlMatch.Success)
                proj.ProjectUrl = urlMatch.Value.Trim();

            var dateRange = FindDateRange(block.RawText);
            proj.StartDate = dateRange.Item1;
            proj.EndDate = dateRange.Item2;

            // Rest forms description
            var descLines = blockLines.Skip(1).Where(l => !IsDateRangeLine(l) && !l.Contains(proj.ProjectUrl ?? "___invalid___")).ToList();
            proj.Description = string.Join(Environment.NewLine, descLines).Trim();

            if (!string.IsNullOrWhiteSpace(proj.Name))
                target.Projects.Add(proj);
        }
    }

    private void ParseSocialLinks(DetectedResumeSection section, ParsedResumeDto target)
    {
        ExtractSocialLinksFromText(section.RawContent, target);
    }

    private void ParseGenericText(string text, ParsedResumeDto target)
    {
        // Check if there are any emails or phone numbers in unclassified content
        var emailMatch = Regex.Match(text, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
        if (emailMatch.Success && string.IsNullOrWhiteSpace(target.Email))
            target.Email = emailMatch.Value;

        var phoneMatch = Regex.Match(text, @"(\+?\d[\d\s\-().]{7,}\d)");
        if (phoneMatch.Success && string.IsNullOrWhiteSpace(target.PhoneNumber))
            target.PhoneNumber = phoneMatch.Value.Trim();

        ExtractSocialLinksFromText(text, target);
    }

    private void ExtractSocialLinksFromText(string text, ParsedResumeDto target)
    {
        foreach (Match m in Regex.Matches(text, @"(https?://)?(www\.)?(linkedin\.com|github\.com|stackoverflow\.com|twitter\.com)[^\s\)\u00A0]+", RegexOptions.IgnoreCase))
        {
            var url = m.Value.Trim();
            var platform = url.Contains("linkedin") ? "LinkedIn"
                         : url.Contains("github") ? "GitHub"
                         : url.Contains("stackoverflow") ? "StackOverflow"
                         : "Twitter";

            if (!target.SocialLinks.Any(l => l.Url == url))
                target.SocialLinks.Add(new ParsedSocialLinkDto { PlatformName = platform, Url = url });
        }
    }

    private static (DateTime?, DateTime?) FindDateRange(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return (null, null);

        string datePartPattern = @"\b(?:(Jan(?:uary)?|Feb(?:ruary)?|Mar(?:ch)?|Apr(?:il)?|May|Jun(?:e)?|Jul(?:y)?|Aug(?:ust)?|Sep(?:tember)?|Oct(?:ober)?|Nov(?:ember)?|Dec(?:ember)?)\b\.?\s+)?(\d{4})\b";

        var rangePattern = new Regex(
            $@"{datePartPattern}\s*(?:[-–—]|to)\s*(?:{datePartPattern}|(Present|Current|Now|Ongoing))",
            RegexOptions.IgnoreCase);

        var match = rangePattern.Match(text);
        if (!match.Success)
        {
            var singleMatch = Regex.Match(text, datePartPattern, RegexOptions.IgnoreCase);
            if (singleMatch.Success)
            {
                var singleDate = ParseDatePart(singleMatch.Groups[1].Value, singleMatch.Groups[2].Value);
                return (singleDate, null);
            }
            return (null, null);
        }

        DateTime? startDate = ParseDatePart(match.Groups[1].Value, match.Groups[2].Value);
        DateTime? endDate = null;

        var presentGroup = match.Groups[5].Value;
        var endYearGroup = match.Groups[4].Value;

        if (!string.IsNullOrWhiteSpace(presentGroup))
        {
            endDate = null;
        }
        else if (!string.IsNullOrWhiteSpace(endYearGroup))
        {
            endDate = ParseDatePart(match.Groups[3].Value, endYearGroup);
        }

        return (startDate, endDate);
    }

    private static DateTime? ParseDatePart(string monthStr, string yearStr)
    {
        if (!int.TryParse(yearStr, out int year)) return null;

        int month = 1;
        if (!string.IsNullOrWhiteSpace(monthStr))
        {
            var m = monthStr.ToLowerInvariant();
            if (m.StartsWith("jan")) month = 1;
            else if (m.StartsWith("feb")) month = 2;
            else if (m.StartsWith("mar")) month = 3;
            else if (m.StartsWith("apr")) month = 4;
            else if (m.StartsWith("may")) month = 5;
            else if (m.StartsWith("jun")) month = 6;
            else if (m.StartsWith("jul")) month = 7;
            else if (m.StartsWith("aug")) month = 8;
            else if (m.StartsWith("sep")) month = 9;
            else if (m.StartsWith("oct")) month = 10;
            else if (m.StartsWith("nov")) month = 11;
            else if (m.StartsWith("dec")) month = 12;
        }

        try
        {
            return new DateTime(year, month, 1);
        }
        catch
        {
            return null;
        }
    }

    private static bool IsDateRangeLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return false;
        return line.Length < 45 && FindDateRange(line).Item1 != null;
    }
}
