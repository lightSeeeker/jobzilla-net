using System.Text;
using DocumentFormat.OpenXml.Packaging;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using jobzilla_net.Application.Resumes.Interfaces;
using Microsoft.Extensions.Logging;

namespace jobzilla_net.Infrasture.Services.Resumes;

public class BasicResumeFileExtractor : IResumeFileExtractor
{
    private readonly ILogger<BasicResumeFileExtractor> _logger;

    public BasicResumeFileExtractor(ILogger<BasicResumeFileExtractor> logger)
    {
        _logger = logger;
    }

    public async Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Resume file not found.", filePath);

        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        _logger.LogInformation("Extracting text from {FilePath} (type: {Ext})", filePath, extension);

        return extension switch
        {
            ".pdf"  => await ExtractFromPdfAsync(filePath),
            ".docx" => await ExtractFromDocxAsync(filePath),
            ".doc"  => await ExtractFromDocxAsync(filePath), // best-effort for legacy .doc
            ".txt"  => await File.ReadAllTextAsync(filePath, cancellationToken),
            _ => throw new NotSupportedException($"File type '{extension}' is not supported for text extraction.")
        };
    }

    private Task<string> ExtractFromPdfAsync(string filePath)
    {
        var sb = new StringBuilder();

        using var reader = new PdfReader(filePath);
        using var pdf = new PdfDocument(reader);

        for (int page = 1; page <= pdf.GetNumberOfPages(); page++)
        {
            var strategy = new LocationTextExtractionStrategy();
            var text = PdfTextExtractor.GetTextFromPage(pdf.GetPage(page), strategy);
            sb.AppendLine(text);
        }

        return Task.FromResult(sb.ToString());
    }

    private Task<string> ExtractFromDocxAsync(string filePath)
    {
        var sb = new StringBuilder();

        using var doc = WordprocessingDocument.Open(filePath, false);
        var body = doc.MainDocumentPart?.Document?.Body;

        if (body != null)
        {
            foreach (var para in body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
            {
                sb.AppendLine(para.InnerText);
            }
        }

        return Task.FromResult(sb.ToString());
    }
}
