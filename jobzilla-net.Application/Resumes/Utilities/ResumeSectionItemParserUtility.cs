using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using jobzilla_net.Application.Resumes.Dtos;
using jobzilla_net.Application.Resumes.Interfaces;

namespace jobzilla_net.Application.Resumes.Utilities;

public static class ResumeSectionItemParserUtility
{
    private static readonly Dictionary<string, string> SkillCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        { "C#", "Programming Languages" }, { "Java", "Programming Languages" }, { "Python", "Programming Languages" }, { "PHP", "Programming Languages" }, { "JavaScript", "Programming Languages" }, { "TypeScript", "Programming Languages" },
        { "ASP.NET Core", "Frameworks" }, { "Django", "Frameworks" }, { "Spring", "Frameworks" }, { "Laravel", "Frameworks" }, { "React", "Frameworks" }, { "Angular", "Frameworks" },
        { "SQL Server", "Databases" }, { "Oracle", "Databases" }, { "PostgreSQL", "Databases" }, { "MongoDB", "Databases" }, { "Cosmos DB", "Databases" },
        { "Azure", "Cloud" }, { "AWS", "Cloud" }, { "GCP", "Cloud" }, { "Firebase", "Cloud" },
        { "Git", "Tools" }, { "GitHub", "Tools" }, { "Azure DevOps", "Tools" }, { "Postman", "Tools" }, { "SSMS", "Tools" }, { "Visual Studio", "Tools" },
        { "Agile", "Methodologies" }, { "Scrum", "Methodologies" }, { "CI/CD", "Methodologies" }, { "Clean Architecture", "Methodologies" }, { "Repository Pattern", "Methodologies" },
        { "HTML", "Frontend" }, { "CSS", "Frontend" }, { "Bootstrap", "Frontend" }, { "Razor Pages", "Frontend" },
        { "SignalR", "APIs & Protocols" }, { "REST API", "APIs & Protocols" }, { "gRPC", "APIs & Protocols" }, { "GraphQL", "APIs & Protocols" },
        { "Sitefinity", "CMS" }, { "Umbraco", "CMS" }, { "WordPress", "CMS" }, { "Sitecore", "CMS" },
        { "xUnit", "Testing" }, { "NUnit", "Testing" }, { "Selenium", "Testing" }, { "Cypress", "Testing" }, { "Unit Testing", "Testing" }, { "Integration Testing", "Testing" },
        { "JWT", "Authentication" }, { "OAuth", "Authentication" }, { "Microsoft Entra ID", "Authentication" }, { "Azure AD", "Authentication" }, { "Role-Based Authorization", "Authentication" }
    };

    private static readonly Dictionary<string, string> SkillNormalizationMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "ASP.Net core", "ASP.NET Core" },
        { "EntityFramework", "Entity Framework Core" },
        { "Entity Framework", "Entity Framework Core" },
        { "SQL server", "SQL Server" },
        { "azure ad", "Microsoft Entra ID (Azure AD)" },
        { "C Sharp", "C#" }
    };

    public static void ParseSectionInto(DetectedResumeSection section, ParsedResumeDto target)
    {
        switch (section.SectionType)
        {
            case ResumeSectionType.PersonalInfo:
                ParsePersonalInfo(section, target.PersonalInfo);
                break;

            case ResumeSectionType.Summary:
                target.Summary = section.RawContent.Trim();
                // We should limit summary to 1000 characters and strip internal newlines
                target.Summary = Regex.Replace(target.Summary, @"\s+", " ");
                if (target.Summary.Length > 1000) target.Summary = target.Summary.Substring(0, 1000);
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
                ParseSocialLinks(section, target.PersonalInfo);
                break;

            case ResumeSectionType.Unknown:
                ParseGenericText(section.RawContent, target);
                break;
        }
    }

    private static void ParsePersonalInfo(DetectedResumeSection section, ParsedPersonalInfoDto target)
    {
        // 1. Email
        var emailMatch = Regex.Match(section.RawContent, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
        if (emailMatch.Success) target.Email = emailMatch.Value.ToLower().Trim();

        // 2. Phone
        var phoneMatch = Regex.Match(section.RawContent, @"\+?[\d\s\-\(\)]{7,20}");
        if (phoneMatch.Success)
        {
            var rawPhone = phoneMatch.Value.Trim();
            // Normalize to E.164-ish
            var digitsOnly = Regex.Replace(rawPhone, @"[^\d+]", "");
            target.Phone = digitsOnly;
        }

        // 3. URLs
        ExtractSocialLinksFromText(section.RawContent, target);

        // 4. Location
        var lines = section.Blocks.SelectMany(b => b.Lines).ToList();
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (Regex.IsMatch(trimmed, @"^[a-zA-Z\s]+,\s*[a-zA-Z\s]+$") && !trimmed.Contains("@"))
            {
                target.Location = trimmed; // City, Country
            }
        }

        // 5. Name Detection: First non-blank line
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(target.FullName) &&
                !trimmed.Contains('@') &&
                !Regex.IsMatch(trimmed, @"\d{3}") &&
                trimmed.Length >= 2 && trimmed.Length <= 60 &&
                !Regex.IsMatch(trimmed, @"^(RESUME|CV|CURRICULUM|PROFILE|SUMMARY|EXPERIENCE|EDUCATION|CONTACT)$", RegexOptions.IgnoreCase))
            {
                target.FullName = trimmed;
                var nameParts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length >= 1)
                {
                    target.FirstName = nameParts[0];
                    if (nameParts.Length > 1)
                    {
                        target.LastName = string.Join(" ", nameParts.Skip(1));
                    }
                }
                break;
            }
        }
    }

    private static void ParseExperience(DetectedResumeSection section, ParsedResumeDto target)
    {
        foreach (var block in section.Blocks)
        {
            if (block.Lines.Count == 0) continue;

            var exp = new ParsedExperienceDto();
            var blockLines = block.Lines;
            var titleLine = blockLines[0];
            
            var atSplit = Regex.Split(titleLine, @"\bat\b|\||–|-", RegexOptions.IgnoreCase);
            if (atSplit.Length >= 2)
            {
                exp.Title = atSplit[0].Trim();
                exp.Company = atSplit[1].Trim();
            }
            else
            {
                exp.Title = titleLine.Trim();
            }

            var (startDate, endDate, isCurrent) = FindDateRangeStr(block.RawText);
            exp.StartDate = startDate;
            exp.EndDate = endDate;
            exp.IsCurrent = isCurrent;

            // Extract bullets
            var descLines = blockLines.Skip(1).Where(l => !IsDateRangeLine(l)).ToList();
            List<string> bullets = new List<string>();
            string currentBullet = "";
            int continuationCount = 0;

            foreach (var line in descLines)
            {
                var trimmed = line.Trim();
                if (Regex.IsMatch(trimmed, @"^[•\-\*–·\d\.]"))
                {
                    if (!string.IsNullOrWhiteSpace(currentBullet)) bullets.Add(currentBullet.Trim());
                    currentBullet = Regex.Replace(trimmed, @"^[•\-\*–·\d\.]+\s*", "");
                    continuationCount = 0;
                }
                else if (!string.IsNullOrWhiteSpace(currentBullet) && continuationCount < 3)
                {
                    currentBullet += " " + trimmed;
                    continuationCount++;
                }
            }
            if (!string.IsNullOrWhiteSpace(currentBullet)) bullets.Add(currentBullet.Trim());
            exp.Bullets = bullets;

            if (!string.IsNullOrWhiteSpace(exp.Title))
                target.Experience.Add(exp);
        }
    }

    private static void ParseEducation(DetectedResumeSection section, ParsedResumeDto target)
    {
        foreach (var block in section.Blocks)
        {
            if (block.Lines.Count == 0) continue;
            var blockLines = block.Lines;
            var edu = new ParsedEducationDto();
            
            edu.Institution = blockLines[0].Trim();
            
            // Fuzzy match degree
            var tokens = block.RawText.Split(new[] { ' ', ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                if (IsDegreeMatch(token, "Bachelor", new[] { "B.S.", "BS", "BSc", "Bachelor", "Bachelors", "Bechelors", "B.E." })) { edu.Degree = "Bachelor's"; break; }
                if (IsDegreeMatch(token, "Master", new[] { "M.S.", "MS", "MSc", "Master", "Masters" })) { edu.Degree = "Master's"; break; }
                if (IsDegreeMatch(token, "Doctorate", new[] { "PhD", "Ph.D", "Doctorate" })) { edu.Degree = "Doctorate"; break; }
                if (IsDegreeMatch(token, "Diploma", new[] { "Diploma", "HND", "Associate" })) { edu.Degree = "Diploma/Associate"; break; }
            }

            var fieldMatch = Regex.Match(block.RawText, @"(?:in|of|major in)\s+([A-Z][a-zA-Z\s]{3,40})", RegexOptions.IgnoreCase);
            if (fieldMatch.Success) edu.Field = fieldMatch.Groups[1].Value.Trim();

            var (startDate, endDate, _) = FindDateRangeStr(block.RawText);
            if (!string.IsNullOrWhiteSpace(startDate) && int.TryParse(startDate.Substring(0,4), out int startY)) edu.StartYear = startY;
            if (!string.IsNullOrWhiteSpace(endDate) && int.TryParse(endDate.Substring(0,4), out int endY)) edu.EndYear = endY;

            if (!string.IsNullOrWhiteSpace(edu.Institution))
                target.Education.Add(edu);
        }
    }

    private static bool IsDegreeMatch(string token, string standardized, string[] aliases)
    {
        foreach (var alias in aliases)
        {
            if (string.Equals(token, alias, StringComparison.OrdinalIgnoreCase) || LevenshteinDistance(token.ToLower(), alias.ToLower()) <= 2)
                return true;
        }
        return false;
    }

    private static void ParseSkills(DetectedResumeSection section, ParsedResumeDto target)
    {
        var tokens = Regex.Split(section.RawContent, @"[,;\|]").Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s));
        foreach (var token in tokens)
        {
            var cleaned = Regex.Replace(token, @"^[\s\-•*]+", "").Trim();
            if (string.IsNullOrWhiteSpace(cleaned)) continue;

            if (SkillNormalizationMap.TryGetValue(cleaned, out string? norm))
                cleaned = norm;

            string category = "Other";
            if (SkillCategories.TryGetValue(cleaned, out string? cat))
                category = cat;

            if (!target.Skills.ContainsKey(category)) target.Skills[category] = new List<string>();
            if (!target.Skills[category].Contains(cleaned, StringComparer.OrdinalIgnoreCase))
                target.Skills[category].Add(cleaned);
        }
    }

    private static void ParseProjects(DetectedResumeSection section, ParsedResumeDto target)
    {
        foreach (var block in section.Blocks)
        {
            if (block.Lines.Count == 0) continue;
            var proj = new ParsedProjectDto();
            var titleLine = block.Lines[0];
            
            var split = Regex.Split(titleLine, @"[-–:|]", RegexOptions.IgnoreCase);
            if (split.Length >= 2) { proj.Name = split[0].Trim(); }
            else { proj.Name = titleLine.Trim(); }

            var clientMatch = Regex.Match(titleLine, @"\(([^)]+)\)");
            if (clientMatch.Success) proj.Client = clientMatch.Groups[1].Value.Trim();

            var techMatch = Regex.Match(block.RawText, @"Technologies:\s*([^\n]+)", RegexOptions.IgnoreCase);
            if (techMatch.Success)
            {
                proj.Technologies = techMatch.Groups[1].Value.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            }

            var focusMatch = Regex.Match(block.RawText, @"Focus:\s*([^\n]+)", RegexOptions.IgnoreCase);
            if (focusMatch.Success)
            {
                proj.Highlights.Add(focusMatch.Groups[1].Value.Trim());
            }

            var numMatch = Regex.Matches(block.RawText, @"\d\.\s*([^\n]+)");
            foreach (Match m in numMatch)
            {
                proj.Highlights.Add(m.Groups[1].Value.Trim());
            }

            if (!string.IsNullOrWhiteSpace(proj.Name))
                target.Projects.Add(proj);
        }
    }

    private static void ParseCertifications(DetectedResumeSection section, ParsedResumeDto target)
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
            }
            var (startDate, _, _) = FindDateRangeStr(trimmed);
            cert.IssueDate = startDate;
            if (!string.IsNullOrWhiteSpace(cert.Name)) target.Certifications.Add(cert);
        }
    }

    private static void ParseSocialLinks(DetectedResumeSection section, ParsedPersonalInfoDto target)
    {
        ExtractSocialLinksFromText(section.RawContent, target);
    }

    private static void ParseGenericText(string text, ParsedResumeDto target)
    {
        var emailMatch = Regex.Match(text, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
        if (emailMatch.Success && string.IsNullOrWhiteSpace(target.PersonalInfo.Email))
            target.PersonalInfo.Email = emailMatch.Value.ToLower().Trim();

        var phoneMatch = Regex.Match(text, @"\+?[\d\s\-\(\)]{7,20}");
        if (phoneMatch.Success && string.IsNullOrWhiteSpace(target.PersonalInfo.Phone))
        {
            target.PersonalInfo.Phone = Regex.Replace(phoneMatch.Value.Trim(), @"[^\d+]", "");
        }

        ExtractSocialLinksFromText(text, target.PersonalInfo);
        
        // Try extract experience if dates and companies present
        var dateMatches = FindDateRangeStr(text);
        if (dateMatches.Item1 != null && target.Experience.Count == 0)
        {
            // Simple heuristic fallback to flag experience block
        }
    }

    private static void ExtractSocialLinksFromText(string text, ParsedPersonalInfoDto target)
    {
        foreach (Match m in Regex.Matches(text, @"(https?://)?(www\.)?(linkedin\.com|github\.com|gitlab\.com|behance\.net)[^\s\)\u00A0]+", RegexOptions.IgnoreCase))
        {
            var url = m.Value.Trim();
            if (url.Contains("linkedin")) target.Linkedin = url;
            else if (url.Contains("github") || url.Contains("gitlab")) target.Github = url;
            else target.Portfolio = url;
        }
    }

    private static (string?, string?, bool) FindDateRangeStr(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return (null, null, false);
        string datePartPattern = @"(?:(Jan(?:uary)?|Feb(?:ruary)?|Mar(?:ch)?|Apr(?:il)?|May|Jun(?:e)?|Jul(?:y)?|Aug(?:ust)?|Sep(?:tember)?|Oct(?:ober)?|Nov(?:ember)?|Dec(?:ember)?)\b\.?\s*)?(\d{4})";
        var rangePattern = new Regex($@"{datePartPattern}\s*(?:[-–—]|to|till)\s*(?:{datePartPattern}|(Present|Current|Now|Till Date))", RegexOptions.IgnoreCase);
        var match = rangePattern.Match(text);
        if (match.Success)
        {
            string? startDate = FormatYYYYMM(match.Groups[1].Value, match.Groups[2].Value);
            string? endDate = null;
            bool isCurrent = false;
            var presentGroup = match.Groups[5].Value;
            var endYearGroup = match.Groups[4].Value;
            if (!string.IsNullOrWhiteSpace(presentGroup)) isCurrent = true;
            else if (!string.IsNullOrWhiteSpace(endYearGroup)) endDate = FormatYYYYMM(match.Groups[3].Value, endYearGroup);
            return (startDate, endDate, isCurrent);
        }
        
        var singleMatch = Regex.Match(text, datePartPattern, RegexOptions.IgnoreCase);
        if (singleMatch.Success)
        {
            return (FormatYYYYMM(singleMatch.Groups[1].Value, singleMatch.Groups[2].Value), null, false);
        }
        return (null, null, false);
    }

    private static string? FormatYYYYMM(string monthStr, string yearStr)
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
            return $"{year}-{month:D2}";
        }
        return $"{year}";
    }

    private static bool IsDateRangeLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return false;
        return line.Length < 45 && FindDateRangeStr(line).Item1 != null;
    }

    private static int LevenshteinDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        if (n == 0) return m;
        if (m == 0) return n;

        for (int i = 0; i <= n; d[i, 0] = i++) { }
        for (int j = 0; j <= m; d[0, j] = j++) { }

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m];
    }
}
