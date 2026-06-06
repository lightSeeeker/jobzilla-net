using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using jobzilla_net.Application.Resumes.Dtos;
using jobzilla_net.Application.Resumes.Interfaces;
using Microsoft.Extensions.Logging;

namespace jobzilla_net.Application.Resumes.Utilities;

public static class ResumeTextParserUtility
{
    public static Task<ParsedResumeDto> ParseAsync(string rawText, CancellationToken cancellationToken = default)
    {

        var dto = new ParsedResumeDto();

        if (string.IsNullOrWhiteSpace(rawText))
            return Task.FromResult(dto);

        // 1. Extract global emails and phone numbers across full text first as fallback
        var emailMatch = Regex.Match(rawText, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
        if (emailMatch.Success)
            dto.PersonalInfo.Email = emailMatch.Value.ToLower().Trim();

        var phoneMatch = Regex.Match(rawText, @"\+?[\d\s\-\(\)]{7,20}");
        if (phoneMatch.Success)
            dto.PersonalInfo.Phone = Regex.Replace(phoneMatch.Value.Trim(), @"[^\d+]", "");

        // 2. Call dynamic section detector to group text blocks by section
        var document = ResumeSectionDetectorUtility.DetectSections(rawText);

        // 3. Process each detected section using modular item parser
        foreach (var section in document.Sections)
        {
            ResumeSectionItemParserUtility.ParseSectionInto(section, dto);
        }

        // 4. Ensure we have the global name set if the item parser didn't extract it
        if (string.IsNullOrWhiteSpace(dto.PersonalInfo.FullName))
        {
            var lines = rawText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var line in lines.Take(10))
            {
                if (!line.Contains('@') && !Regex.IsMatch(line, @"\d{3}") &&
                    line.Length >= 2 && line.Length <= 60 &&
                    !Regex.IsMatch(line, @"^(RESUME|CV|CURRICULUM|PROFILE|SUMMARY|EXPERIENCE|EDUCATION|CONTACT)$", RegexOptions.IgnoreCase))
                {
                    dto.PersonalInfo.FullName = line.Trim();
                    var nameParts = dto.PersonalInfo.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (nameParts.Length >= 1)
                    {
                        dto.PersonalInfo.FirstName = nameParts[0];
                        if (nameParts.Length > 1)
                        {
                            dto.PersonalInfo.LastName = string.Join(" ", nameParts.Skip(1));
                        }
                    }
                    break;
                }
            }
        }

        return Task.FromResult(dto);
    }
}
