namespace jobzilla_net.Core.Enums;

/// <summary>
/// Origin of a resume template, which determines how it is rendered.
/// System templates are hand-authored Razor views compiled into the app.
/// Custom templates are HTML files uploaded from the admin panel and rendered
/// via placeholder-token substitution (no server-side code execution).
/// </summary>
public enum ResumeTemplateSource
{
    System = 0,
    Custom = 1
}
