using jobzilla_net.Application.Resumes.Dtos;
using jobzilla_net.Application.Resumes.ViewModels;

namespace jobzilla_net.Application.Resumes.Interfaces;

/// <summary>
/// Produces the final resume HTML for a given template + candidate data, hiding
/// whether the template is a built-in Razor view (System) or an admin-uploaded
/// HTML file rendered via placeholder tokens (Custom).
/// </summary>
public interface IResumeHtmlComposer
{
    Task<string> ComposeAsync(ResumeTemplateDto template, ResumeExportViewModel model, CancellationToken cancellationToken = default);
}
