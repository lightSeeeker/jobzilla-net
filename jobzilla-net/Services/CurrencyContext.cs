using jobzilla_net.Application.Common.Interfaces;

namespace jobzilla_net.Services;

/// <summary>
/// Per-request display-currency state for views. Resolves the currency from a
/// cookie (manual override) or IP geo (default), fetches the base→display rate
/// once, then formats salaries synchronously for the razor views.
/// </summary>
public class CurrencyContext
{
    public const string CookieName = "displayCurrency";

    private readonly IHttpContextAccessor _http;
    private readonly ICurrencyService _currency;
    private bool _initialized;
    private decimal _rate = 1m;

    public string CurrencyCode { get; private set; } = "QAR";
    public string Symbol { get; private set; } = "QR";
    public string BaseCurrency => _currency.BaseCurrency;
    public IReadOnlyList<CurrencyOption> Options => _currency.SupportedCurrencies;
    public bool IsConverted => !string.Equals(CurrencyCode, BaseCurrency, StringComparison.OrdinalIgnoreCase);

    public CurrencyContext(IHttpContextAccessor http, ICurrencyService currency)
    {
        _http = http;
        _currency = currency;
        CurrencyCode = currency.BaseCurrency;
        Symbol = currency.GetOption(currency.BaseCurrency).Symbol;
    }

    public async Task EnsureInitializedAsync()
    {
        if (_initialized) return;
        _initialized = true;

        var ctx = _http.HttpContext;
        var code = ctx?.Request.Cookies[CookieName];

        if (string.IsNullOrEmpty(code))
        {
            code = await _currency.ResolveCurrencyForIpAsync(GetClientIp(ctx));
            // Persist so subsequent requests skip the geo lookup.
            ctx?.Response.Cookies.Append(CookieName, code, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                IsEssential = true,
                Path = "/"
            });
        }

        var rate = await _currency.GetRateFromBaseAsync(code!);
        if (rate is null)
        {
            // Rate unavailable → show base currency rather than a wrong number.
            code = _currency.BaseCurrency;
            rate = 1m;
        }

        var opt = _currency.GetOption(code!);
        CurrencyCode = opt.Code;
        Symbol = opt.Symbol;
        _rate = rate.Value;
    }

    public string Format(decimal amount) =>
        $"{Symbol} {Math.Round(amount * _rate, MidpointRounding.AwayFromZero):N0}";

    public string FormatRange(decimal? min, decimal? max)
    {
        if (!min.HasValue && !max.HasValue) return "Negotiable";
        if (min.HasValue && max.HasValue) return $"{Format(min.Value)} - {Format(max.Value)}";
        return Format((min ?? max)!.Value);
    }

    private static string? GetClientIp(HttpContext? ctx)
    {
        if (ctx is null) return null;
        var forwarded = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwarded))
            return forwarded.Split(',')[0].Trim();
        return ctx.Connection.RemoteIpAddress?.ToString();
    }
}
