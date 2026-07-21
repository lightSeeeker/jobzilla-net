using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using jobzilla_net.Application.Resumes.Dtos;
using jobzilla_net.Application.Resumes.Interfaces;
using jobzilla_net.Application.Resumes.ViewModels;
using jobzilla_net.Core.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace jobzilla_net.Infrasture.Services.Resumes;

/// <summary>
/// Renders a resume template to HTML. System templates go through the Razor engine;
/// Custom (admin-uploaded) templates are HTML files rendered by substituting
/// placeholder tokens with the candidate's data. Custom templates never execute
/// server-side code — only string substitution — so untrusted uploads are safe.
/// </summary>
public class ResumeHtmlComposer : IResumeHtmlComposer
{
    private readonly ITemplateRenderer _razorRenderer;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ResumeHtmlComposer> _logger;

    private static readonly Regex BlockRegex =
        new(@"\{\{#(\w+)\}\}(.*?)\{\{/\1\}\}", RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex TokenRegex =
        new(@"\{\{\s*(\w+)\s*\}\}", RegexOptions.Compiled);

    public ResumeHtmlComposer(
        ITemplateRenderer razorRenderer,
        IWebHostEnvironment env,
        ILogger<ResumeHtmlComposer> logger)
    {
        _razorRenderer = razorRenderer;
        _env = env;
        _logger = logger;
    }

    public async Task<string> ComposeAsync(ResumeTemplateDto template, ResumeExportViewModel model, CancellationToken cancellationToken = default)
    {
        if (template.Source == ResumeTemplateSource.System)
        {
            return await _razorRenderer.RenderTemplateAsync(
                $"~/Views/Shared/ResumeTemplates/{template.TemplateFilePath}.cshtml",
                model);
        }

        var relative = template.TemplateFilePath.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar);
        var physicalPath = Path.Combine(_env.WebRootPath, relative);

        if (!File.Exists(physicalPath))
        {
            _logger.LogWarning("Custom template file not found at {Path} for template {TemplateId}", physicalPath, template.Id);
            return "<html><body><p>Template file is missing. Please contact the administrator.</p></body></html>";
        }

        var html = await File.ReadAllTextAsync(physicalPath, cancellationToken);
        return FillTokens(html, model);
    }

    private string FillTokens(string html, ResumeExportViewModel model)
    {
        var sections = model.GetUnifiedSections().ToList();

        // 1. Repeat section blocks: {{#Experience}} ... {{/Experience}}
        html = BlockRegex.Replace(html, match =>
        {
            var sectionName = match.Groups[1].Value;
            var inner = match.Groups[2].Value;

            var matching = sections
                .Where(s => s.SectionType.ToString().Equals(sectionName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matching.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var section in matching)
            {
                foreach (var item in section.Items.OrderBy(i => i.DisplayOrder))
                {
                    var itemHtml = inner.Replace("{{SectionTitle}}", Encode(section.Title));
                    itemHtml = TokenRegex.Replace(itemHtml, m =>
                    {
                        var key = m.Groups[1].Value;
                        return Encode(Format(key, item.GetValue(key)));
                    });
                    sb.Append(itemHtml);
                }
            }
            return sb.ToString();
        });

        // 2. Scalar (top-level) tokens.
        var scalars = BuildScalarMap(model);
        html = TokenRegex.Replace(html, m =>
        {
            var key = m.Groups[1].Value;
            return scalars.TryGetValue(key, out var val) ? val : string.Empty;
        });

        return html;
    }

    private Dictionary<string, string> BuildScalarMap(ResumeExportViewModel model)
    {
        var p = model.Profile;
        var imageUri = model.GetProfileImageDataUri(_env.WebRootPath);

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["FullName"] = Encode(p.FullName),
            ["Title"] = Encode(p.ProfessionalTitle),
            ["ProfessionalTitle"] = Encode(p.ProfessionalTitle),
            ["Email"] = Encode(p.Email),
            ["Phone"] = Encode(p.PhoneNumber),
            ["PhoneNumber"] = Encode(p.PhoneNumber),
            ["Location"] = Encode(p.Location),
            ["Summary"] = Encode(p.Summary),
            ["ExperienceYears"] = Encode(p.ExperienceYears?.ToString()),
            // ProfileImage resolves to a src-ready value (data URI or path); already safe.
            ["ProfileImage"] = imageUri ?? string.Empty
        };
    }

    private static string Format(string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        if (key.Contains("Date", StringComparison.OrdinalIgnoreCase)
            && DateTime.TryParse(value, out var date))
        {
            return date.ToString("MMM yyyy");
        }

        return value;
    }

    private static string Encode(string? value) =>
        string.IsNullOrEmpty(value) ? string.Empty : WebUtility.HtmlEncode(value);
}
