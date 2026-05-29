namespace jobzilla_net.Application.Resumes.Dtos;

/// <summary>
/// The normalized, template-independent resume document.
/// This is the single source of truth for the Builder UI and template rendering.
/// Sections are ordered by DisplayOrder and rendered dynamically — no hardcoding.
/// </summary>
public class ResumeDocument
{
    // ── Personal Information (flat, always present) ────────────────────────────
    public string? FullName { get; set; }
    public string? ProfessionalTitle { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? Website { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? Summary { get; set; }

    // ── Dynamic ordered sections ───────────────────────────────────────────────
    public List<ResumeSection> Sections { get; set; } = new();

    // ── Helpers ───────────────────────────────────────────────────────────────
    public IEnumerable<ResumeSection> VisibleSections =>
        Sections.Where(s => s.IsVisible).OrderBy(s => s.DisplayOrder);
}
