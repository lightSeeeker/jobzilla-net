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

            // Read the optional per-template sidebar directive (after polyfill, so the colour
            // var is already resolved to a hex — respects the user's colour customization).
            var underlay = ParseSidebarUnderlay(html);

            // Generate PDF from HTML
            var pdfBytes = await _pdfGenerator.GeneratePdfFromHtmlAsync(html, cancellationToken);

            // Two-column templates: paint the sidebar colour full-height behind content on every
            // page so the left column reaches the bottom edge even on a partial last page.
            if (pdfBytes != null && underlay != null)
            {
                try
                {
                    var (r, g, b, fraction) = underlay.Value;
                    pdfBytes = PdfSidebarUnderlay.Apply(pdfBytes, r, g, b, fraction);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Sidebar underlay post-processing failed; returning base PDF.");
                }
            }

            return pdfBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export resume to PDF for user {UserId}", userId);
            return null;
        }
    }

    /// <summary>
    /// Replaces CSS var(--name) usages with their actual values for PDF engines that
    /// lack CSS custom property support (SelectPdf drops the whole declaration, even
    /// when a fallback argument is present).
    /// </summary>
    private string PolyfillCssVariablesForPdf(string html)
    {
        var variables = new Dictionary<string, string>();
        var rootBlockRegex = new System.Text.RegularExpressions.Regex(@":root\s*{([^}]*)}");
        var varDefRegex = new System.Text.RegularExpressions.Regex(@"(--[\w-]+)\s*:\s*([^;}]+);?");
        var varUsageRegex = new System.Text.RegularExpressions.Regex(@"var\s*\(\s*(--[\w-]+)\s*(?:,\s*((?:[^()]|\([^()]*\))*))?\)");

        foreach (System.Text.RegularExpressions.Match rootMatch in rootBlockRegex.Matches(html))
        {
            foreach (System.Text.RegularExpressions.Match varMatch in varDefRegex.Matches(rootMatch.Groups[1].Value))
            {
                // Later blocks overwrite earlier ones (custom settings overwrite defaults)
                variables[varMatch.Groups[1].Value.Trim()] = varMatch.Groups[2].Value.Trim();
            }
        }

        // Definitions may reference other variables; resolve until stable.
        for (var pass = 0; pass < 4; pass++)
        {
            var changed = false;
            foreach (var key in variables.Keys.ToList())
            {
                variables[key] = varUsageRegex.Replace(variables[key], m =>
                {
                    if (variables.TryGetValue(m.Groups[1].Value, out var v)) { changed = true; return v; }
                    if (m.Groups[2].Success) { changed = true; return m.Groups[2].Value.Trim(); }
                    return m.Value;
                });
            }
            if (!changed) break;
        }

        return varUsageRegex.Replace(html, match =>
        {
            if (variables.TryGetValue(match.Groups[1].Value, out var val)) return val;
            return match.Groups[2].Success ? match.Groups[2].Value.Trim() : match.Value;
        });
    }

    /// <summary>
    /// Reads a template's optional sidebar-underlay directive, emitted (export only) as
    /// <c>&lt;meta name="cvb-pdf-sidebar" content="#RRGGBB|0.35" /&gt;</c>. The colour is written
    /// as <c>var(--x)</c> in the template so <see cref="PolyfillCssVariablesForPdf"/> resolves it
    /// to the actual (possibly user-customized) hex before this runs. Returns null when absent.
    /// </summary>
    private static (int r, int g, int b, double fraction)? ParseSidebarUnderlay(string html)
    {
        var m = System.Text.RegularExpressions.Regex.Match(
            html,
            @"<meta\s+name=[""']cvb-pdf-sidebar[""']\s+content=[""']\s*#?([0-9a-fA-F]{3}|[0-9a-fA-F]{6})\s*\|\s*([0-9]*\.?[0-9]+)\s*[""']",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!m.Success) return null;

        var hex = m.Groups[1].Value;
        if (hex.Length == 3)
            hex = string.Concat(hex[0], hex[0], hex[1], hex[1], hex[2], hex[2]);

        if (!double.TryParse(m.Groups[2].Value, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var fraction))
            return null;
        if (fraction <= 0 || fraction >= 1) return null;

        var r = Convert.ToInt32(hex.Substring(0, 2), 16);
        var g = Convert.ToInt32(hex.Substring(2, 2), 16);
        var b = Convert.ToInt32(hex.Substring(4, 2), 16);
        return (r, g, b, fraction);
    }

    /// <summary>
    /// Appends print-specific CSS so the PDF faithfully reflects the on-screen design:
    /// zero body margin (each template owns its spacing), forced background-color
    /// printing (colored sidebars/headers), and legacy page-break hygiene so entries
    /// stay whole — covers custom uploaded templates that skip _ResumeBaseStyles.
    /// </summary>
    private static string InjectPrintCss(string html)
    {
        const string css =
            "<style>" +
            "html,body{margin:0 !important;padding:0 !important;}" +
            "*{-webkit-print-color-adjust:exact !important;print-color-adjust:exact !important;}" +
            ".resume-item{page-break-inside:avoid;}" +
            ".resume-section-title,.resume-item-header{page-break-after:avoid;}" +
            "</style>";

        var headCloseIndex = html.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
        return headCloseIndex >= 0
            ? html.Insert(headCloseIndex, css)
            : css + html; // fragment without <head> (e.g. some custom templates)
    }
}
