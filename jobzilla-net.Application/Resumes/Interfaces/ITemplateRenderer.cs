namespace jobzilla_net.Application.Resumes.Interfaces;

/// <summary>
/// Responsible for rendering a template (e.g., Razor view) into an HTML string.
/// This output can be used for previewing in the browser or converting to PDF later.
/// </summary>
public interface ITemplateRenderer
{
    Task<string> RenderTemplateAsync<TModel>(string viewName, TModel model);
}
