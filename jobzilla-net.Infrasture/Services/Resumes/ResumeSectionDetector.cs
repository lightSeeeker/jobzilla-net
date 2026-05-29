using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using jobzilla_net.Application.Resumes.Dtos;
using jobzilla_net.Application.Resumes.Interfaces;

namespace jobzilla_net.Infrasture.Services.Resumes;

public class ResumeSectionDetector : IResumeSectionDetector
{
    private static readonly Dictionary<string, ResumeSectionType> SectionKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        // Personal Info / Contact
        { "contact", ResumeSectionType.PersonalInfo },
        { "contact info", ResumeSectionType.PersonalInfo },
        { "contact information", ResumeSectionType.PersonalInfo },
        { "personal info", ResumeSectionType.PersonalInfo },
        { "personal information", ResumeSectionType.PersonalInfo },
        { "personal details", ResumeSectionType.PersonalInfo },
        { "address", ResumeSectionType.PersonalInfo },
        { "candidate information", ResumeSectionType.PersonalInfo },

        // Summary / Profile
        { "summary", ResumeSectionType.Summary },
        { "professional summary", ResumeSectionType.Summary },
        { "career summary", ResumeSectionType.Summary },
        { "summary of qualifications", ResumeSectionType.Summary },
        { "objective", ResumeSectionType.Summary },
        { "career objective", ResumeSectionType.Summary },
        { "profile", ResumeSectionType.Summary },
        { "professional profile", ResumeSectionType.Summary },
        { "about me", ResumeSectionType.Summary },
        { "about", ResumeSectionType.Summary },

        // Experience
        { "experience", ResumeSectionType.Experience },
        { "work experience", ResumeSectionType.Experience },
        { "professional experience", ResumeSectionType.Experience },
        { "employment history", ResumeSectionType.Experience },
        { "work history", ResumeSectionType.Experience },
        { "career history", ResumeSectionType.Experience },
        { "professional background", ResumeSectionType.Experience },
        { "experience history", ResumeSectionType.Experience },
        { "jobs", ResumeSectionType.Experience },
        { "employment", ResumeSectionType.Experience },
        { "professional record", ResumeSectionType.Experience },

        // Education
        { "education", ResumeSectionType.Education },
        { "academic", ResumeSectionType.Education },
        { "academic history", ResumeSectionType.Education },
        { "academic background", ResumeSectionType.Education },
        { "qualifications", ResumeSectionType.Education },
        { "educational background", ResumeSectionType.Education },
        { "education & credentials", ResumeSectionType.Education },
        { "education history", ResumeSectionType.Education },
        { "degrees", ResumeSectionType.Education },
        { "academic qualifications", ResumeSectionType.Education },

        // Skills
        { "skills", ResumeSectionType.Skills },
        { "technical skills", ResumeSectionType.Skills },
        { "core competencies", ResumeSectionType.Skills },
        { "areas of expertise", ResumeSectionType.Skills },
        { "expertise", ResumeSectionType.Skills },
        { "technologies", ResumeSectionType.Skills },
        { "key skills", ResumeSectionType.Skills },
        { "professional skills", ResumeSectionType.Skills },
        { "skills & tools", ResumeSectionType.Skills },
        { "skills summary", ResumeSectionType.Skills },
        { "technologies & skills", ResumeSectionType.Skills },
        { "languages & technologies", ResumeSectionType.Skills },
        { "it skills", ResumeSectionType.Skills },

        // Certifications
        { "certifications", ResumeSectionType.Certifications },
        { "certification", ResumeSectionType.Certifications },
        { "licenses", ResumeSectionType.Certifications },
        { "licenses & certifications", ResumeSectionType.Certifications },
        { "credentials", ResumeSectionType.Certifications },
        { "certificates", ResumeSectionType.Certifications },
        { "courses", ResumeSectionType.Certifications },
        { "professional development", ResumeSectionType.Certifications },

        // Projects
        { "projects", ResumeSectionType.Projects },
        { "personal projects", ResumeSectionType.Projects },
        { "academic projects", ResumeSectionType.Projects },
        { "key projects", ResumeSectionType.Projects },
        { "selected projects", ResumeSectionType.Projects },
        { "portfolio", ResumeSectionType.Projects },
        { "project experience", ResumeSectionType.Projects },

        // Languages
        { "languages", ResumeSectionType.Languages },
        { "language proficiency", ResumeSectionType.Languages },
        { "spoken languages", ResumeSectionType.Languages },

        // Achievements
        { "achievements", ResumeSectionType.Achievements },
        { "key achievements", ResumeSectionType.Achievements },
        { "accomplishments", ResumeSectionType.Achievements },
        { "major achievements", ResumeSectionType.Achievements },

        // Awards
        { "awards", ResumeSectionType.Awards },
        { "honors", ResumeSectionType.Awards },
        { "awards & honors", ResumeSectionType.Awards },
        { "honorable mentions", ResumeSectionType.Awards },

        // Volunteer
        { "volunteer", ResumeSectionType.Volunteer },
        { "volunteer experience", ResumeSectionType.Volunteer },
        { "volunteering", ResumeSectionType.Volunteer },
        { "community service", ResumeSectionType.Volunteer },

        // Publications
        { "publications", ResumeSectionType.Publications },
        { "research", ResumeSectionType.Publications },
        { "patents", ResumeSectionType.Publications },
        { "papers", ResumeSectionType.Publications },
        { "books", ResumeSectionType.Publications },

        // Social Links
        { "social", ResumeSectionType.SocialLinks },
        { "links", ResumeSectionType.SocialLinks },
        { "social links", ResumeSectionType.SocialLinks },
        { "social profiles", ResumeSectionType.SocialLinks },
        { "profiles", ResumeSectionType.SocialLinks }
    };

    private static readonly string[] CustomSectionKeywords = new[]
    {
        "interests", "hobbies", "references", "affiliations", "activities",
        "extracurricular", "extracurricular activities", "memberships",
        "additional", "additional information", "other info", "training"
    };

    public ParsedResumeDocument DetectSections(string rawText)
    {
        var document = new ParsedResumeDocument { RawText = rawText };

        if (string.IsNullOrWhiteSpace(rawText))
            return document;

        // Split raw text into individual lines and filter out header/footer noise
        var lines = rawText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
            .Where(line => !IsHeaderOrFooterLine(line))
            .ToArray();
        var matches = new List<HeadingMatch>();

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cleaned = CleanHeadingText(line);
            if (string.IsNullOrWhiteSpace(cleaned)) continue;

            bool isHeading = false;
            ResumeSectionType type = ResumeSectionType.Unknown;

            // Rule 1: Matches known section keyword exactly
            if (SectionKeywords.TryGetValue(cleaned, out type))
            {
                isHeading = true;
            }
            // Rule 2: Matches custom/unknown section keywords exactly
            else if (CustomSectionKeywords.Contains(cleaned, StringComparer.OrdinalIgnoreCase))
            {
                isHeading = true;
                type = ResumeSectionType.Unknown;
            }
            // Rule 3: Line has markdown heading format (e.g. ## Heading or ### Heading)
            else if (Regex.IsMatch(line.Trim(), @"^#{1,4}\s+.+$"))
            {
                isHeading = true;
                var rawClean = Regex.Replace(line.Trim(), @"^#+\s+", "");
                var mappedClean = CleanHeadingText(rawClean);
                if (SectionKeywords.TryGetValue(mappedClean, out var t))
                {
                    type = t;
                }
                else
                {
                    type = ResumeSectionType.Unknown;
                }
            }
            // Rule 4: Followed by a separator line (e.g. ------ or ======)
            else if (i + 1 < lines.Length && IsSeparatorLine(lines[i + 1]))
            {
                isHeading = true;
                if (SectionKeywords.TryGetValue(cleaned, out var t))
                {
                    type = t;
                }
                else
                {
                    type = ResumeSectionType.Unknown;
                }
            }

            if (isHeading)
            {
                matches.Add(new HeadingMatch
                {
                    LineIndex = i,
                    Type = type,
                    RawHeading = cleaned
                });
            }
        }

        // Now split the text using the detected headings
        if (matches.Count == 0)
        {
            // If no headings detected, put everything in PersonalInfo
            var personalInfoSection = new DetectedResumeSection
            {
                SectionType = ResumeSectionType.PersonalInfo,
                RawHeading = "Personal Information",
                RawContent = rawText
            };
            personalInfoSection.Blocks = ExtractBlocks(lines);
            document.Sections.Add(personalInfoSection);
        }
        else
        {
            // Handle pre-heading content (if any) as PersonalInfo
            var firstHeadingIndex = matches[0].LineIndex;
            if (firstHeadingIndex > 0)
            {
                var preLines = lines.Take(firstHeadingIndex).ToArray();
                var preText = string.Join(Environment.NewLine, preLines);
                if (!string.IsNullOrWhiteSpace(preText))
                {
                    var personalInfoSection = new DetectedResumeSection
                    {
                        SectionType = ResumeSectionType.PersonalInfo,
                        RawHeading = "Personal Information",
                        RawContent = preText
                    };
                    personalInfoSection.Blocks = ExtractBlocks(preLines);
                    document.Sections.Add(personalInfoSection);
                }
            }

            // Create sections for each heading
            for (int k = 0; k < matches.Count; k++)
            {
                var currentMatch = matches[k];
                var nextMatchIndex = (k + 1 < matches.Count) ? matches[k + 1].LineIndex : lines.Length;

                // Content starts after the heading line
                int contentStart = currentMatch.LineIndex + 1;
                // If followed by a separator line, skip it as well
                if (contentStart < lines.Length && IsSeparatorLine(lines[contentStart]))
                {
                    contentStart++;
                }

                int contentEnd = nextMatchIndex;
                // If the next heading is preceded by a separator line, exclude it from content
                if (contentEnd - 1 > contentStart && IsSeparatorLine(lines[contentEnd - 1]))
                {
                    contentEnd--;
                }

                var sectionLines = lines.Skip(contentStart).Take(contentEnd - contentStart).ToArray();
                var sectionContentText = string.Join(Environment.NewLine, sectionLines);

                var section = new DetectedResumeSection
                {
                    SectionType = currentMatch.Type,
                    RawHeading = currentMatch.RawHeading,
                    RawContent = sectionContentText
                };
                section.Blocks = ExtractBlocks(sectionLines);

                document.Sections.Add(section);
            }
        }

        return document;
    }

    private static string CleanHeadingText(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var cleaned = raw.Trim();

        // Strip surrounding Markdown stars, underscores, brackets, etc.
        cleaned = Regex.Replace(cleaned, @"^[#*=\-\[\]\s:]+", "");
        cleaned = Regex.Replace(cleaned, @"[#*=\-\[\]\s:]+$", "");

        return cleaned.Trim();
    }

    private static bool IsSeparatorLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return false;
        var trimmed = line.Trim();
        if (trimmed.Length < 3) return false;

        // Matches lines consisting solely of hyphens, equals, asterisks, underscores, tildes, hashes
        return Regex.IsMatch(trimmed, @"^[=\-\*_#~]+$");
    }

    private static bool IsHeaderOrFooterLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return false;
        var trimmed = line.Trim();
        
        // Matches "Page X", "Page X of Y", "X of Y", "1 / 3" or lines containing just page numbers
        if (Regex.IsMatch(trimmed, @"^page\s*[-–]?\s*\d+(\s*of\s*\d+)?$", RegexOptions.IgnoreCase))
            return true;
        if (Regex.IsMatch(trimmed, @"^\d+\s*/\s*\d+$"))
            return true;
        if (Regex.IsMatch(trimmed, @"^-\s*\d+\s*-$"))
            return true;
        if (Regex.IsMatch(trimmed, @"^[\d\s]+$") && trimmed.Length <= 3) // single numbers
            return true;
            
        return false;
    }

    private static List<DetectedSectionBlock> ExtractBlocks(IEnumerable<string> lines)
    {
        var blocks = new List<DetectedSectionBlock>();
        var currentBlockLines = new List<string>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                if (currentBlockLines.Count > 0)
                {
                    blocks.Add(new DetectedSectionBlock { Lines = currentBlockLines });
                    currentBlockLines = new List<string>();
                }
            }
            else
            {
                currentBlockLines.Add(line);
            }
        }

        if (currentBlockLines.Count > 0)
        {
            blocks.Add(new DetectedSectionBlock { Lines = currentBlockLines });
        }

        return blocks;
    }

    private class HeadingMatch
    {
        public int LineIndex { get; set; }
        public ResumeSectionType Type { get; set; }
        public string RawHeading { get; set; } = string.Empty;
    }
}
