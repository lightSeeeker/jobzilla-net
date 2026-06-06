using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using jobzilla_net.Application.Resumes.Interfaces;
using Microsoft.Extensions.Logging;

namespace jobzilla_net.Infrasture.Services.Resumes;

public static class ResumeFileExtractorUtility
{
    public static async Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Resume file not found.", filePath);

        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".pdf"  => await ExtractFromPdfAsync(filePath),
            ".docx" => await ExtractFromDocxAsync(filePath),
            ".doc"  => await ExtractFromDocxAsync(filePath), // best-effort for legacy .doc
            ".txt"  => await File.ReadAllTextAsync(filePath, cancellationToken),
            _ => throw new NotSupportedException($"File type '{extension}' is not supported for text extraction.")
        };
    }

    private static Task<string> ExtractFromPdfAsync(string filePath)
    {
        var sb = new StringBuilder();

        using var reader = new PdfReader(filePath);
        using var pdf = new PdfDocument(reader);

        for (int page = 1; page <= pdf.GetNumberOfPages(); page++)
        {
            var strategy = new ColumnAwareTextExtractionStrategy();
            var text = PdfTextExtractor.GetTextFromPage(pdf.GetPage(page), strategy);
            
            // Normalize multiple blank lines and spaces
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\r\n|\r", "\n");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\n{3,}", "\n\n");
            
            sb.AppendLine(text);
        }

        return Task.FromResult(sb.ToString());
    }

    private static Task<string> ExtractFromDocxAsync(string filePath)
    {
        var sb = new StringBuilder();

        using var doc = WordprocessingDocument.Open(filePath, false);
        var body = doc.MainDocumentPart?.Document?.Body;

        if (body != null)
        {
            foreach (var element in body.ChildElements)
            {
                if (element is DocumentFormat.OpenXml.Wordprocessing.Paragraph para)
                {
                    sb.AppendLine(para.InnerText);
                }
                else if (element is DocumentFormat.OpenXml.Wordprocessing.Table table)
                {
                    sb.AppendLine();
                    foreach (var cell in table.Descendants<DocumentFormat.OpenXml.Wordprocessing.TableCell>())
                    {
                        var cellText = cell.InnerText.Trim();
                        if (!string.IsNullOrWhiteSpace(cellText))
                        {
                            sb.AppendLine(cellText);
                        }
                    }
                    sb.AppendLine();
                }
            }
        }

        return Task.FromResult(sb.ToString());
    }

    private class ColumnAwareTextExtractionStrategy : ITextExtractionStrategy
    {
        private readonly List<TextChunk> _chunks = new();

        public void EventOccurred(iText.Kernel.Pdf.Canvas.Parser.Data.IEventData data, iText.Kernel.Pdf.Canvas.Parser.EventType type)
        {
            if (type == iText.Kernel.Pdf.Canvas.Parser.EventType.RENDER_TEXT)
            {
                var renderInfo = (iText.Kernel.Pdf.Canvas.Parser.Data.TextRenderInfo)data;
                var startPoint = renderInfo.GetBaseline().GetStartPoint();
                var endPoint = renderInfo.GetAscentLine().GetEndPoint();
                
                _chunks.Add(new TextChunk
                {
                    Text = renderInfo.GetText(),
                    X = startPoint.Get(0),
                    Y = startPoint.Get(1),
                    Right = endPoint.Get(0)
                });
            }
        }

        public ICollection<iText.Kernel.Pdf.Canvas.Parser.EventType> GetSupportedEvents()
        {
            return new[] { iText.Kernel.Pdf.Canvas.Parser.EventType.RENDER_TEXT };
        }

        public string GetResultantText()
        {
            if (_chunks.Count == 0) return string.Empty;

            // Group chunks into columns by rounding X to nearest 150 points (~2 inches).
            // Then order by Column (left to right), then Y (top to bottom).
            var sortedChunks = _chunks
                .OrderBy(c => Math.Round(c.X / 150.0) * 150)
                .ThenByDescending(c => c.Y)
                .ThenBy(c => c.X)
                .ToList();

            var sb = new StringBuilder();
            float lastY = -1;
            float lastX = -1;

            foreach (var chunk in sortedChunks)
            {
                if (lastY != -1 && Math.Abs(chunk.Y - lastY) > 5)
                {
                    sb.AppendLine();
                }
                else if (lastX != -1 && chunk.X - lastX > 10)
                {
                    sb.Append(" ");
                }
                
                sb.Append(chunk.Text);
                
                lastY = chunk.Y;
                lastX = chunk.Right;
            }

            return sb.ToString();
        }

        private class TextChunk
        {
            public string Text { get; set; } = string.Empty;
            public float X { get; set; }
            public float Y { get; set; }
            public float Right { get; set; }
        }
    }
}
