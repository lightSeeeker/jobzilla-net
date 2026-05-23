using System.ComponentModel.DataAnnotations;

namespace jobzilla_net.Models.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }

    /// <summary>
    /// Populated from the active tab: "Candidate" or "Employer".
    /// Informational only at login — role is loaded from Identity claims.
    /// </summary>
    public string? UserType { get; set; }

    /// <summary>
    /// URL to redirect to after a successful login.
    /// </summary>
    public string? ReturnUrl { get; set; }
}
