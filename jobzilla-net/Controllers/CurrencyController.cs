using jobzilla_net.Application.Common.Interfaces;
using jobzilla_net.Services;
using Microsoft.AspNetCore.Mvc;

namespace jobzilla_net.Controllers;

public class CurrencyController : Controller
{
    private readonly ICurrencyService _currency;

    public CurrencyController(ICurrencyService currency) => _currency = currency;

    [HttpGet]
    public IActionResult Set(string code, string? returnUrl = null)
    {
        var opt = _currency.SupportedCurrencies
            .FirstOrDefault(o => string.Equals(o.Code, code, StringComparison.OrdinalIgnoreCase));

        if (opt != null)
        {
            Response.Cookies.Append(CurrencyContext.CookieName, opt.Code, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                IsEssential = true,
                Path = "/"
            });
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }
}
