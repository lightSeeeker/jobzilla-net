using System;
using System.Collections.Generic;

namespace jobzilla_net.Application.Resumes.Dtos;

public class ParsedResumeDocument
{
    public string RawText { get; set; } = string.Empty;
    public List<DetectedResumeSection> Sections { get; set; } = new();
}

public class DetectedResumeSection
{
    public ResumeSectionType SectionType { get; set; }
    public string RawHeading { get; set; } = string.Empty;
    public string RawContent { get; set; } = string.Empty;
    public List<DetectedSectionBlock> Blocks { get; set; } = new();
}

public class DetectedSectionBlock
{
    public List<string> Lines { get; set; } = new();
    public string RawText => string.Join(Environment.NewLine, Lines);
}
