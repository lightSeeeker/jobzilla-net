using jobzilla_net.Application.Resumes.Interfaces;
using Microsoft.Extensions.Logging;
using SelectPdf;

namespace jobzilla_net.Infrasture.Services.Resumes;

public class SelectPdfGenerator : IPdfGenerator
{
    private readonly ILogger<SelectPdfGenerator> _logger;

    public SelectPdfGenerator(ILogger<SelectPdfGenerator> logger)
    {
        _logger = logger;
    }

    public Task<byte[]> GeneratePdfFromHtmlAsync(string htmlContent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating PDF from HTML string using SelectPdf.");

        var converter = new HtmlToPdf();
        
        // Setup PDF options
        converter.Options.PdfPageSize = PdfPageSize.A4;
        converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
        converter.Options.MarginLeft = 30;
        converter.Options.MarginRight = 30;
        converter.Options.MarginTop = 30;
        converter.Options.MarginBottom = 30;

        // Optionally disable JavaScript execution during rendering if not needed, for security/speed
        converter.Options.JavaScriptEnabled = false;

        // Convert the HTML string
        var doc = converter.ConvertHtmlString(htmlContent);
        
        using var stream = new MemoryStream();
        doc.Save(stream);
        doc.Close();
        
        return Task.FromResult(stream.ToArray());
    }
}
