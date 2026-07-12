using jobzilla_net.Application.Resumes.Interfaces;
using Microsoft.Extensions.Logging;
using SelectPdf;

namespace jobzilla_net.Infrasture.Services.Resumes;

public class SelectPdfGenerator : IPdfGenerator
{
    // A4 portrait printable area at 96 DPI = 210mm ≈ 794px. Rendering the page at
    // exactly this width (instead of SelectPdf's 1024px default) makes the layout
    // map 1:1 onto the page — no down-scaling, so fonts and spacing come out at the
    // sizes the template designed for.
    private const int A4WidthPx = 794;

    private readonly ILogger<SelectPdfGenerator> _logger;

    public SelectPdfGenerator(ILogger<SelectPdfGenerator> logger)
    {
        _logger = logger;
    }

    public Task<byte[]> GeneratePdfFromHtmlAsync(string htmlContent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating PDF from HTML string using SelectPdf.");

        var converter = new HtmlToPdf();

        converter.Options.PdfPageSize = PdfPageSize.A4;
        converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

        // Zero outer margins: spacing is owned by each template's own CSS (padding /
        // full-bleed sidebars). A blanket page margin would double the padding on
        // text templates and leave a white border around full-bleed designs.
        converter.Options.MarginLeft = 0;
        converter.Options.MarginRight = 0;
        converter.Options.MarginTop = 0;
        converter.Options.MarginBottom = 0;

        // Render at true page width so the design is 1:1 with the page.
        converter.Options.WebPageWidth = A4WidthPx;
        converter.Options.WebPageHeight = 0; // auto — let content flow across pages

        // Give web fonts / remote CSS a moment to load before rasterizing.
        converter.Options.MinPageLoadTime = 1;

        // Templates are static HTML/CSS; no JS needed (faster, safer).
        converter.Options.JavaScriptEnabled = false;

        var doc = converter.ConvertHtmlString(htmlContent);

        try
        {
            using var stream = new MemoryStream();
            doc.Save(stream);
            return Task.FromResult(stream.ToArray());
        }
        finally
        {
            doc.Close();
        }
    }
}
