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

        // Zero margins on all sides so full-bleed designs (e.g. Modern Professional's
        // dark sidebar) reach every edge — top, bottom and left — on every page.
        // Each template owns its own inner spacing via padding. A non-zero page margin
        // would leave white bands around the sidebar and shave usable height (spilling
        // content onto a near-empty extra page).
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
