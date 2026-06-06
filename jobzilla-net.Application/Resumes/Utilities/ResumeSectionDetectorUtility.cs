using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using jobzilla_net.Application.Resumes.Dtos;
using jobzilla_net.Application.Resumes.Interfaces;

namespace jobzilla_net.Application.Resumes.Utilities;

public static class ResumeSectionDetectorUtility
{
    private static readonly Dictionary<string, ResumeSectionType> CanonicalSections = new(StringComparer.OrdinalIgnoreCase)
    {
        { "PROFILE", ResumeSectionType.Summary },
        { "SUMMARY", ResumeSectionType.Summary },
        { "OBJECTIVE", ResumeSectionType.Summary },
        { "ABOUT ME", ResumeSectionType.Summary },
        { "PROFESSIONAL SUMMARY", ResumeSectionType.Summary },
        { "CAREER SUMMARY", ResumeSectionType.Summary },
        { "EXECUTIVE SUMMARY", ResumeSectionType.Summary },
        
        { "EXPERIENCE", ResumeSectionType.Experience },
        { "WORK EXPERIENCE", ResumeSectionType.Experience },
        { "PROFESSIONAL EXPERIENCE", ResumeSectionType.Experience },
        { "EMPLOYMENT HISTORY", ResumeSectionType.Experience },
        { "WORK HISTORY", ResumeSectionType.Experience },
        { "CAREER HISTORY", ResumeSectionType.Experience },
        { "EMPLOYMENT", ResumeSectionType.Experience },

        { "EDUCATION", ResumeSectionType.Education },
        { "ACADEMIC BACKGROUND", ResumeSectionType.Education },
        { "QUALIFICATIONS", ResumeSectionType.Education },
        { "ACADEMIC HISTORY", ResumeSectionType.Education },

        { "SKILLS", ResumeSectionType.Skills },
        { "TECHNICAL SKILLS", ResumeSectionType.Skills },
        { "CORE COMPETENCIES", ResumeSectionType.Skills },
        { "KEY SKILLS", ResumeSectionType.Skills },
        { "TECHNOLOGIES", ResumeSectionType.Skills },
        { "TECH STACK", ResumeSectionType.Skills },

        { "PROJECTS", ResumeSectionType.Projects },
        { "KEY PROJECTS", ResumeSectionType.Projects },
        { "NOTABLE PROJECTS", ResumeSectionType.Projects },
        { "PORTFOLIO", ResumeSectionType.Projects },
        { "PROJECT EXPERIENCE", ResumeSectionType.Projects },

        { "CONTACT", ResumeSectionType.PersonalInfo },
        { "CONTACT INFORMATION", ResumeSectionType.PersonalInfo },
        { "PERSONAL DETAILS", ResumeSectionType.PersonalInfo },
        { "GET IN TOUCH", ResumeSectionType.PersonalInfo },

        { "CERTIFICATIONS", ResumeSectionType.Certifications },
        { "COURSES", ResumeSectionType.Certifications },
        { "LICENSES", ResumeSectionType.Certifications },
        { "AWARDS", ResumeSectionType.Certifications },
        { "ACHIEVEMENTS", ResumeSectionType.Certifications },

        { "LANGUAGES", ResumeSectionType.Languages },
        { "LANGUAGE PROFICIENCY", ResumeSectionType.Languages },

        { "REFERENCES", ResumeSectionType.References },
        { "REFERENCES AVAILABLE UPON REQUEST", ResumeSectionType.References }
    };

    public static ParsedResumeDocument DetectSections(string rawText)
    {
        var document = new ParsedResumeDocument();
        if (string.IsNullOrWhiteSpace(rawText)) return document;

        var lines = rawText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        
        var currentSection = new DetectedResumeSection { SectionType = ResumeSectionType.Unknown };
        var currentBlock = new DetectedSectionBlock();

        foreach (var originalLine in lines)
        {
            var trimmed = originalLine.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                if (currentBlock.Lines.Count > 0)
                {
                    currentSection.Blocks.Add(currentBlock);
                    currentBlock = new DetectedSectionBlock();
                }
                continue;
            }

            var matchedType = TryMatchHeading(trimmed);

            if (matchedType != null)
            {
                // Push previous block if has data
                if (currentBlock.Lines.Count > 0)
                {
                    currentSection.Blocks.Add(currentBlock);
                    currentBlock = new DetectedSectionBlock();
                }
                // Push previous section
                if (currentSection.Blocks.Count > 0 || currentSection.SectionType != ResumeSectionType.Unknown)
                {
                    document.Sections.Add(currentSection);
                }

                currentSection = new DetectedResumeSection { SectionType = matchedType.Value };
                // Optionally store the heading line itself in the block or skip it.
                // We skip adding it to block lines to avoid parsing headings as job titles.
                continue;
            }

            currentBlock.Lines.Add(originalLine);
            currentSection.RawContent += originalLine + Environment.NewLine;
        }

        if (currentBlock.Lines.Count > 0)
        {
            currentSection.Blocks.Add(currentBlock);
        }
        if (currentSection.Blocks.Count > 0 || currentSection.SectionType != ResumeSectionType.Unknown)
        {
            document.Sections.Add(currentSection);
        }

        return document;
    }

    private static ResumeSectionType? TryMatchHeading(string line)
    {
        var cleaned = Regex.Replace(line, @"^[#*=\-\[\]\s:]+", "");
        cleaned = Regex.Replace(cleaned, @"[#*=\-\[\]\s:]+$", "").Trim();
        
        if (cleaned.Length > 45 || cleaned.Length < 3) return null;

        // Is ALL CAPS or Title Case (no sentence punctuation)?
        bool isAllCaps = cleaned.ToUpperInvariant() == cleaned && Regex.IsMatch(cleaned, @"[A-Z]");
        bool isTitleCase = !Regex.IsMatch(cleaned, @"[.!?]$");

        if (!isAllCaps && !isTitleCase) return null;

        // Perfect dictionary match
        if (CanonicalSections.TryGetValue(cleaned, out var sectionType))
        {
            return sectionType;
        }

        // Partial match for known keywords if ALL CAPS or short Title Case
        if (isAllCaps || (isTitleCase && cleaned.Length < 30))
        {
            var tokens = cleaned.ToUpperInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                if (CanonicalSections.TryGetValue(token, out var st))
                {
                    // If a single word matches (like EXPERIENCE or SKILLS), treat it as a match
                    if (token == "EXPERIENCE" || token == "SKILLS" || token == "EDUCATION" || token == "PROFILE" || token == "PROJECTS")
                    {
                        return st;
                    }
                }
            }
        }

        return null;
    }
}
