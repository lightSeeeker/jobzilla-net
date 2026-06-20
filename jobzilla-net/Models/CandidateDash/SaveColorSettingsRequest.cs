namespace jobzilla_net.Models.Candidate;

/// <summary>
/// Request body for POST /Candidate/SaveColorSettings.
/// Carries the selected palette color variables so they can be persisted
/// into <c>CandidateResume.SettingsJson</c> without a page reload.
/// </summary>
public class SaveColorSettingsRequest
{
    public int ResumeId { get; set; }
    public int TemplateId { get; set; }

    /// <summary>
    /// CSS custom property name → hex value, e.g. <c>"--mp-accent": "#48a9a6"</c>.
    /// </summary>
    public Dictionary<string, string> Colors { get; set; } = new();
}
