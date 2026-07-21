using System.Text.Json;
using jobzilla_net.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace jobzilla_net.Infrasture.Services;

/// <summary>
/// Fetches live FX rates (open.er-api.com, no key) and resolves a visitor's
/// currency from their IP (ipapi.co). Both results are cached to avoid
/// per-request outbound calls. Base currency is QAR.
/// </summary>
public class CurrencyService : ICurrencyService
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CurrencyService> _logger;

    private const string RatesCacheKey = "fx:rates";

    public string BaseCurrency => "QAR";

    private static readonly IReadOnlyList<CurrencyOption> _supported = new List<CurrencyOption>
    {
        new("QAR", "Qatari Riyal",       "QR"),
        new("USD", "US Dollar",          "$"),
        new("EUR", "Euro",               "€"),
        new("GBP", "British Pound",      "£"),
        new("PKR", "Pakistani Rupee",    "₨"),
        new("INR", "Indian Rupee",       "₹"),
        new("AED", "UAE Dirham",         "AED"),
        new("SAR", "Saudi Riyal",        "SR"),
        new("BDT", "Bangladeshi Taka",   "৳"),
        new("PHP", "Philippine Peso",    "₱"),
        new("IDR", "Indonesian Rupiah",  "Rp"),
        new("MYR", "Malaysian Ringgit",  "RM"),
        new("EGP", "Egyptian Pound",     "E£"),
        new("TRY", "Turkish Lira",       "₺"),
        new("CAD", "Canadian Dollar",    "C$"),
        new("AUD", "Australian Dollar",  "A$"),
        new("JPY", "Japanese Yen",       "¥"),
        new("CNY", "Chinese Yuan",       "¥"),
        new("KRW", "South Korean Won",   "₩"),
        new("SGD", "Singapore Dollar",   "S$"),
        new("ZAR", "South African Rand", "R"),
        new("NGN", "Nigerian Naira",     "₦"),
        new("BRL", "Brazilian Real",     "R$"),
        new("RUB", "Russian Ruble",      "₽"),
    };

    private static readonly Dictionary<string, CurrencyOption> _byCode =
        _supported.ToDictionary(o => o.Code, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<CurrencyOption> SupportedCurrencies => _supported;

    public CurrencyService(HttpClient http, IMemoryCache cache, ILogger<CurrencyService> logger)
    {
        _http = http;
        _cache = cache;
        _logger = logger;
    }

    public CurrencyOption GetOption(string code) =>
        code != null && _byCode.TryGetValue(code, out var o) ? o : _byCode[BaseCurrency];

    public async Task<IReadOnlyDictionary<string, decimal>> GetRatesAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(RatesCacheKey, out IReadOnlyDictionary<string, decimal>? cached) && cached != null)
            return cached;

        try
        {
            using var resp = await _http.GetAsync($"https://open.er-api.com/v6/latest/{BaseCurrency}", ct);
            resp.EnsureSuccessStatusCode();
            await using var stream = await resp.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            var dict = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            if (doc.RootElement.TryGetProperty("rates", out var rates))
            {
                foreach (var p in rates.EnumerateObject())
                {
                    if (p.Value.TryGetDecimal(out var v))
                        dict[p.Name] = v;
                }
            }

            if (dict.Count > 0)
            {
                _cache.Set(RatesCacheKey, (IReadOnlyDictionary<string, decimal>)dict, TimeSpan.FromHours(6));
                return dict;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FX rate fetch failed; using base-only fallback.");
        }

        // Fallback: base currency only (1:1) so display never breaks.
        return new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { [BaseCurrency] = 1m };
    }

    public async Task<decimal?> GetRateFromBaseAsync(string toCurrency, CancellationToken ct = default)
    {
        if (string.Equals(toCurrency, BaseCurrency, StringComparison.OrdinalIgnoreCase))
            return 1m;

        var rates = await GetRatesAsync(ct);
        return rates.TryGetValue(toCurrency, out var r) ? r : null;
    }

    public async Task<string> ResolveCurrencyForIpAsync(string? ip, CancellationToken ct = default)
    {
        // No IP or a private/loopback address (e.g. localhost) → base currency.
        if (string.IsNullOrWhiteSpace(ip) || IsPrivate(ip))
            return BaseCurrency;

        var key = $"geo:ccy:{ip}";
        if (_cache.TryGetValue(key, out string? cached) && cached != null)
            return cached;

        var result = BaseCurrency;
        try
        {
            using var resp = await _http.GetAsync($"https://ipapi.co/{ip}/json/", ct);
            if (resp.IsSuccessStatusCode)
            {
                await using var stream = await resp.Content.ReadAsStreamAsync(ct);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
                if (doc.RootElement.TryGetProperty("currency", out var c) &&
                    c.ValueKind == JsonValueKind.String)
                {
                    var code = c.GetString();
                    // Only adopt currencies we can present (symbol/name curated).
                    if (!string.IsNullOrEmpty(code) && _byCode.ContainsKey(code))
                        result = code;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Geo currency lookup failed for {Ip}.", ip);
        }

        _cache.Set(key, result, TimeSpan.FromHours(24));
        return result;
    }

    private static bool IsPrivate(string ip) =>
        ip == "::1" ||
        ip.StartsWith("127.") ||
        ip.StartsWith("10.") ||
        ip.StartsWith("192.168.") ||
        ip.StartsWith("172.16.") ||
        ip.StartsWith("169.254.");
}
