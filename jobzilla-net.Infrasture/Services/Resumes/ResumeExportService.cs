using jobzilla_net.Application.Resumes.Interfaces;
using Microsoft.Extensions.Logging;

namespace jobzilla_net.Infrasture.Services.Resumes;

public class ResumeExportService : IResumeExportService
{
    private readonly IResumeBuilderService _builderService;
    private readonly IResumeHtmlComposer _composer;
    private readonly IPdfGenerator _pdfGenerator;
    private readonly ILogger<ResumeExportService> _logger;

    public ResumeExportService(
        IResumeBuilderService builderService,
        IResumeHtmlComposer composer,
        IPdfGenerator pdfGenerator,
        ILogger<ResumeExportService> logger)
    {
        _builderService = builderService;
        _composer = composer;
        _pdfGenerator = pdfGenerator;
        _logger = logger;
    }

    public async Task<byte[]?> ExportResumeToPdfAsync(string userId, int templateId, int? resumeId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting resume to PDF for user {UserId} using template {TemplateId}", userId, templateId);

        try
        {
            var template = await _builderService.GetTemplateByIdAsync(templateId, cancellationToken);
            if (template == null)
            {
                _logger.LogWarning("Template {TemplateId} not found or inactive.", templateId);
                return null;
            }

            var model = await _builderService.GetResumeDataAsync(userId, resumeId, cancellationToken);
            
            model.TemplateId = templateId;
            model.ResumeId = resumeId;
            model.IsExport = true;

            // Render HTML string (System = Razor, Custom = uploaded HTML with tokens)
            var html = await _composer.ComposeAsync(template, model, cancellationToken);

            // Polyfill CSS Variables for the PDF Generator
            // SelectPdf/wkhtmltopdf engines do not support var(--variable-name) in CSS.
            // We parse the :root blocks and inline the colors directly into the CSS rules.
            html = PolyfillCssVariablesForPdf(html);

            // Inject print-specific CSS so the PDF matches the on-screen design:
            // force background colors to print and avoid ugly page breaks inside items.
            html = InjectPrintCss(html);

            // Generate PDF from HTML
            var pdfBytes = await _pdfGenerator.GeneratePdfFromHtmlAsync(html, cancellationToken);
            
            return pdfBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export resume to PDF for user {UserId}", userId);
            return null;
        }
    }

    /// <summary>
    /// Replaces CSS var(--name) usages with their actual hex values for PDF engines
    /// that lack modern CSS custom property support.
    /// </summary>
    private string PolyfillCssVariablesForPdf(string html)
    {
        var variables = new Dictionary<string, string>();
        var rootBlockRegex = new System.Text.RegularExpressions.Regex(@":root\s*{([^}]+)}");
        var varDefRegex = new System.Text.RegularExpressions.Regex(@"(--[\w-]+)\s*:\s*([^;]+);");

        var rootMatches = rootBlockRegex.Matches(html);
        foreach (System.Text.RegularExpressions.Match rootMatch in rootMatches)
        {
            var block = rootMatch.Groups[1].Value;
            var varMatches = varDefRegex.Matches(block);
            foreach (System.Text.RegularExpressions.Match varMatch in varMatches)
            {
                var varName = varMatch.Groups[1].Value.Trim();
                var varValue = varMatch.Groups[2].Value.Trim();
                // Later blocks overwrite earlier ones (custom settings overwrite defaults)
                variables[varName] = varValue;
            }
        }

        var varUsageRegex = new System.Text.RegularExpressions.Regex(@"var\s*\(\s*(--[\w-]+)\s*\)");
        return varUsageRegex.Replace(html, match =>
        {
            var varName = match.Groups[1].Value.Trim();
            return variables.TryGetValue(varName, out var val) ? val : match.Value;
        });
    }

    /// <summary>
    /// Appends print-specific CSS so the PDF faithfully reflects the on-screen design:
    /// zero page margin (each template owns its spacing) and forced background-color
    /// printing (colored sidebars/headers). No page-break-avoid rules — content flows
    /// to fill each page so faux-column backgrounds never leave an empty gap.
    /// </summary>
    private static string InjectPrintCss(string html)
    {
        const string css =
            "<style>" +
            "html,body{margin:0 !important;padding:0 !important;}" +
            "*{-webkit-print-color-adjust:exact !important;print-color-adjust:exact !important;}" +
            "</style>";

        var headCloseIndex = html.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
        return headCloseIndex >= 0
            ? html.Insert(headCloseIndex, css)
            : css + html; // fragment without <head> (e.g. some custom templates)
    }
}
