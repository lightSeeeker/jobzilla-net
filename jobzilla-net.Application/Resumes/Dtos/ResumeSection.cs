namespace jobzilla_net.Application.Resumes.Dtos;

/// <summary>
/// A named section in a candidate's resume document.
/// Can be a predefined type (Experience, Education…) or a completely custom section.
/// </summary>
public class ResumeSection
{
    /// <summary>Client-side stable ID. Maps to DB entity IDs where applicable.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public ResumeBuilderSectionType SectionType { get; set; } = ResumeBuilderSectionType.Custom;

    /// <summary>User-editable heading shown on the resume.</summary>
    public string Title { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; } = true;

    public List<ResumeSectionItem> Items { get; set; } = new();
}
