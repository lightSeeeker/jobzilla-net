using System.Net.Http.Json;
using System.Text.Json;
using jobzilla_net.Application.Resumes.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace jobzilla_net.Infrasture.Services.Resumes;

/// <summary>
/// Generates PDFs with a hosted headless-Chromium service (Browserless '/pdf'
/// endpoint, which is Puppeteer under the hood). This renders resumes pixel-identical
/// to the browser preview — full CSS (variables, flex/grid, gradients), background
/// colors, and correct page-break behaviour — which the SelectPdf engine cannot do.
///
/// Config (secrets via env var / user-secrets, never appsettings):
///   Pdf:Browserless:Endpoint  e.g. https://production-sfo.browserless.io/pdf  (EU region available)
///   Pdf:Browserless:Token     the API token
/// </summary>
public class BrowserlessPdfGenerator : IPdfGenerator
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BrowserlessPdfGenerator> _logger;
    private readonly string _endpoint;
    private readonly string _token;

    public BrowserlessPdfGenerator(HttpClient httpClient, IConfiguration config, ILogger<BrowserlessPdfGenerator> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _endpoint = config["Pdf:Browserless:Endpoint"] ?? "https://chrome.browserless.io/pdf";
        _token = config["Pdf:Browserless:Token"] ?? string.Empty;
    }

    public async Task<byte[]> GeneratePdfFromHtmlAsync(string htmlContent, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_token))
            throw new InvalidOperationException("Browserless token is not configured (Pdf:Browserless:Token).");

        _logger.LogInformation("Generating PDF via Browserless (headless Chromium).");

        var url = _endpoint.Contains('?')
            ? $"{_endpoint}&token={_token}"
            : $"{_endpoint}?token={_token}";

        // Zero margins + A4; templates own their spacing and use @page/CSS. printBackground
        // makes colored sidebars/headers render; preferCSSPageSize honours template @page.
        var payload = new
        {
            html = htmlContent,
            options = new
            {
                printBackground = true,
                displayHeaderFooter = false,
                format = "A4",
                preferCSSPageSize = true,
                margin = new { top = "0", bottom = "0", left = "0", right = "0" }
            }
        };

        using var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Browserless PDF request failed: {Status} {Body}", response.StatusCode, body);
            throw new HttpRequestException($"Browserless PDF generation failed with status {response.StatusCode}.");
        }

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
