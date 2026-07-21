using jobzilla_net.Infrasture.Identity;
using jobzilla_net.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace jobzilla_net.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager   = userManager;
        _logger        = logger;
    }

    // ── LOGIN ─────────────────────────────────────────────────────────────────

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_signInManager.IsSignedIn(User))
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
                
            return RedirectToDashboard();
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {
        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        if (!ModelState.IsValid)
        {
            if (isAjax)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { success = false, message = string.Join(" ", errors) });
            }
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // Validate that the user actually has the role they selected to log in as (e.g. Candidate vs Employer)
                if (!string.IsNullOrEmpty(model.UserType) && !roles.Contains(model.UserType))
                {
                    await _signInManager.SignOutAsync();
                    string errorMsg = $"Invalid account type. You are not registered as a {model.UserType}.";
                    
                    if (isAjax) return BadRequest(new { success = false, message = errorMsg });
                    
                    ModelState.AddModelError(string.Empty, errorMsg);
                    return View(model);
                }
            }

            _logger.LogInformation("User {Email} logged in.", model.Email);

            if (isAjax)
            {
                string returnUrl = Url.Action("Index", "Home") ?? "/";
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    returnUrl = model.ReturnUrl;
                }
                else if (user is not null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Admin"))     returnUrl = Url.Action("Index", "Admin") ?? "/";
                    else if (roles.Contains("Employer"))  returnUrl = Url.Action("Index", "Employer") ?? "/";
                    else if (roles.Contains("Candidate")) returnUrl = Url.Action("Index", "Candidate") ?? "/";
                }
                return Json(new { success = true, redirectUrl = returnUrl });
            }

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return await RedirectToDashboardAsync(model.Email);
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User {Email} account is locked out.", model.Email);
            if (isAjax) return BadRequest(new { success = false, message = "Your account has been locked. Please try again later." });
            ModelState.AddModelError(string.Empty, "Your account has been locked. Please try again later.");
            return View(model);
        }

        if (isAjax) return BadRequest(new { success = false, message = "Invalid email or password." });
        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(model);
    }

    // ── REGISTER ──────────────────────────────────────────────────────────────

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? userType = null)
    {
        if (_signInManager.IsSignedIn(User))
            return RedirectToDashboard();

        return View(new RegisterViewModel { UserType = userType ?? string.Empty });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken ct)
    {
        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        if (!ModelState.IsValid)
        {
            if (isAjax)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { success = false, message = string.Join(" ", errors) });
            }
            return View(model);
        }

        // Validate UserType is one of the allowed values
        if (model.UserType != "Candidate" && model.UserType != "Employer")
        {
            if (isAjax) return BadRequest(new { success = false, message = "Please select a valid account type." });
            ModelState.AddModelError(nameof(model.UserType), "Please select a valid account type.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName      = model.Email,           // Identity uses email as username
            Email         = model.Email,
            DisplayName   = model.UserName,
            PhoneNumber   = model.PhoneNumber,
            UserType      = model.UserType,
            IsActive      = true,
            EmailConfirmed = true                  // Skip email confirmation for now
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, model.UserType);
            await _signInManager.SignInAsync(user, isPersistent: false);

            _logger.LogInformation("New {UserType} account created for {Email}.", model.UserType, model.Email);

            if (isAjax)
            {
                string returnUrl = Url.Action("Index", "Home") ?? "/";
                if (model.UserType == "Admin")     returnUrl = Url.Action("Index", "Admin") ?? "/";
                else if (model.UserType == "Employer")  returnUrl = Url.Action("Index", "Employer") ?? "/";
                else if (model.UserType == "Candidate") returnUrl = Url.Action("Index", "Candidate") ?? "/";
                return Json(new { success = true, redirectUrl = returnUrl });
            }

            return await RedirectToDashboardAsync(user.Email!);
        }

        if (isAjax)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { success = false, message = string.Join(" ", errors) });
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    // ── EXTERNAL / SOCIAL LOGIN ─────────────────────────────────────────────────

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLogin(string provider, string? userType = null, string? returnUrl = null)
    {
        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
        if (!schemes.Any(s => string.Equals(s.Name, provider, StringComparison.OrdinalIgnoreCase)))
        {
            TempData["AuthError"] = $"{provider} sign-in is not configured yet.";
            return RedirectToAction(nameof(Login));
        }

        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        properties.Items["userType"] = (userType == "Employer") ? "Employer" : "Candidate";
        return Challenge(properties, provider);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError != null)
        {
            _logger.LogWarning("External login error: {Error}", remoteError);
            TempData["AuthError"] = $"Error from external provider: {remoteError}";
            return RedirectToAction(nameof(Login));
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            TempData["AuthError"] = "Could not load external login information.";
            return RedirectToAction(nameof(Login));
        }

        // Already linked → sign straight in.
        var signInResult = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

        if (signInResult.Succeeded)
        {
            _logger.LogInformation("{Provider} user signed in.", info.LoginProvider);
            var existing = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            return await ExternalRedirect(existing, returnUrl);
        }

        // New external user → create (or link to an existing email) then sign in.
        var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var displayName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                          ?? email
                          ?? $"{info.LoginProvider} user";
        var userType = info.AuthenticationProperties?.Items.TryGetValue("userType", out var ut) == true
                       && ut == "Employer" ? "Employer" : "Candidate";

        var user = email != null ? await _userManager.FindByEmailAsync(email) : null;

        if (user == null)
        {
            // Some providers (e.g. Twitter) may not return an email — synthesise a stable one.
            var safeEmail = email ?? $"{info.LoginProvider.ToLowerInvariant()}_{info.ProviderKey}@social.fursanet";
            user = new ApplicationUser
            {
                UserName = safeEmail,
                Email = safeEmail,
                DisplayName = displayName,
                UserType = userType,
                IsActive = true,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                _logger.LogWarning("External user creation failed: {Errors}",
                    string.Join(" ", createResult.Errors.Select(e => e.Description)));
                TempData["AuthError"] = "Could not create an account from your social profile.";
                return RedirectToAction(nameof(Login));
            }

            await _userManager.AddToRoleAsync(user, userType);
        }

        await _userManager.AddLoginAsync(user, info);
        await _signInManager.SignInAsync(user, isPersistent: false);
        _logger.LogInformation("New {Provider} account linked for {Email}.", info.LoginProvider, user.Email);

        return await ExternalRedirect(user, returnUrl);
    }

    // ── LOGOUT ────────────────────────────────────────────────────────────────

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var email = User.Identity?.Name;
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User {Email} logged out.", email);
        return RedirectToAction("Index", "Home");
    }

    // ── ACCESS DENIED ─────────────────────────────────────────────────────────

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    // ── HELPERS ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Redirects to the role-appropriate dashboard.
    /// Uses User.IsInRole() for GET requests (principal already populated).
    /// </summary>
    private IActionResult RedirectToDashboard()
    {
        if (User.IsInRole("Admin"))     return RedirectToAction("Index", "Admin");
        if (User.IsInRole("Employer"))  return RedirectToAction("Index", "Employer");
        if (User.IsInRole("Candidate")) return RedirectToAction("Index", "Candidate");
        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Async version for POST handlers where the User principal is not yet
    /// refreshed after SignInAsync / PasswordSignInAsync.
    /// </summary>
    /// <summary>
    /// Post external-login redirect: honour a local returnUrl, else route to the
    /// role-appropriate dashboard.
    /// </summary>
    private async Task<IActionResult> ExternalRedirect(ApplicationUser? user, string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        if (user?.Email is not null)
            return await RedirectToDashboardAsync(user.Email);

        return RedirectToAction("Index", "Home");
    }

    private async Task<IActionResult> RedirectToDashboardAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is not null)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))     return RedirectToAction("Index", "Admin");
            if (roles.Contains("Employer"))  return RedirectToAction("Index", "Employer");
            if (roles.Contains("Candidate")) return RedirectToAction("Index", "Candidate");
        }
        return RedirectToAction("Index", "Home");
    }
}
