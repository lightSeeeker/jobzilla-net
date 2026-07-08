namespace jobzilla_net.Application.Common.Interfaces;

/// <summary>
/// Real-time currency conversion for salary display. All stored salaries are
/// treated as <see cref="BaseCurrency"/>; conversion is display-only.
/// </summary>
public interface ICurrencyService
{
    /// <summary>Currency that stored salary values are expressed in.</summary>
    string BaseCurrency { get; }

    /// <summary>Currencies offered in the switcher (curated: code, name, symbol).</summary>
    IReadOnlyList<CurrencyOption> SupportedCurrencies { get; }

    /// <summary>Live rates keyed by currency code, relative to the base (cached).</summary>
    Task<IReadOnlyDictionary<string, decimal>> GetRatesAsync(CancellationToken ct = default);

    /// <summary>Rate to multiply a base-currency amount by to get <paramref name="toCurrency"/>. Null if unavailable.</summary>
    Task<decimal?> GetRateFromBaseAsync(string toCurrency, CancellationToken ct = default);

    /// <summary>Best-guess display currency for a visitor IP (geo lookup, cached). Falls back to base.</summary>
    Task<string> ResolveCurrencyForIpAsync(string? ipAddress, CancellationToken ct = default);

    /// <summary>Metadata for a code; falls back to the base currency option.</summary>
    CurrencyOption GetOption(string code);
}

public record CurrencyOption(string Code, string Name, string Symbol);
