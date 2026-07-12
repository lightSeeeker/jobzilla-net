using System.Net.Http.Headers;
using System.Text;
using jobzilla_net.Application.Resumes.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace jobzilla_net.Infrasture.Services.Resumes;

/// <summary>
/// Generates PDFs with a self-hosted Gotenberg instance (Docker), which wraps headless
/// Chromium behind an HTTP API. Free, unlimited, and keeps candidate PII on your own
/// infrastructure. Renders resumes pixel-identical to the browser preview — full CSS,
/// background colors, correct page breaks — unlike the in-process SelectPdf engine.
///
/// Run Gotenberg:  docker run --rm -p 3000:3000 gotenberg/gotenberg:8
/// Config:  Pdf:Gotenberg:BaseUrl  (e.g. http://localhost:3000 or the VPS address)
/// </summary>
public class GotenbergPdfGenerator : IPdfGenerator
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GotenbergPdfGenerator> _logger;
    private readonly string _baseUrl;

    public GotenbergPdfGenerator(HttpClient httpClient, IConfiguration config, ILogger<GotenbergPdfGenerator> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = (config["Pdf:Gotenberg:BaseUrl"] ?? "http://localhost:3000").TrimEnd('/');
    }

    public async Task<byte[]> GeneratePdfFromHtmlAsync(string htmlContent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating PDF via Gotenberg (self-hosted headless Chromium).");

        using var form = new MultipartFormDataContent();

        // The HTML must be sent as a file part literally named "index.html".
        var htmlPart = new StringContent(htmlContent, Encoding.UTF8, "text/html");
        form.Add(htmlPart, "files", "index.html");

        // Zero margins (templates own their spacing), print background colors, and honour
        // any @page rules in the template CSS.
        form.Add(new StringContent("true"), "printBackground");
        form.Add(new StringContent("true"), "preferCssPageSize");
        form.Add(new StringContent("0"), "marginTop");
        form.Add(new StringContent("0"), "marginBottom");
        form.Add(new StringContent("0"), "marginLeft");
        form.Add(new StringContent("0"), "marginRight");
        // Small delay so web fonts / remote CSS finish loading before render.
        form.Add(new StringContent("1s"), "waitDelay");

        var url = $"{_baseUrl}/forms/chromium/convert/html";
        using var response = await _httpClient.PostAsync(url, form, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Gotenberg PDF request failed: {Status} {Body}", response.StatusCode, body);
            throw new HttpRequestException($"Gotenberg PDF generation failed with status {response.StatusCode}.");
        }

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
