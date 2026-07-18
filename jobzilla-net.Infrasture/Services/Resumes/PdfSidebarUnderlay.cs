using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;

namespace jobzilla_net.Infrasture.Services.Resumes;

/// <summary>
/// Paints a full-height sidebar-colour rectangle UNDER the page content on every page of a
/// generated PDF. SelectPdf cannot make a colored column reach the bottom of a partial last
/// page (html/body background, position:fixed, height:100% all stop at content end), so the
/// template's own sidebar cell only colours the content region. This underlay fills the
/// remaining trailing area on every page so the left column reads edge-to-edge top-to-bottom.
/// Drawn behind content, so the sidebar text/right column stay on top untouched.
/// </summary>
public static class PdfSidebarUnderlay
{
    public static byte[] Apply(byte[] pdf, int r, int g, int b, double widthFraction)
    {
        using var input = new MemoryStream(pdf);
        using var output = new MemoryStream();

        var doc = new PdfDocument(new PdfReader(input), new PdfWriter(output));
        var color = new DeviceRgb(r, g, b);

        for (var i = 1; i <= doc.GetNumberOfPages(); i++)
        {
            var page = doc.GetPage(i);
            var size = page.GetPageSize();
            var canvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), doc);
            canvas.SaveState();
            canvas.SetFillColor(color);
            canvas.Rectangle(0, 0, size.GetWidth() * widthFraction, size.GetHeight());
            canvas.Fill();
            canvas.RestoreState();
        }

        doc.Close(); // flushes the writer into `output`
        return output.ToArray();
    }
}
