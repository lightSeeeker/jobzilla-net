using System.ComponentModel.DataAnnotations;

namespace jobzilla_net.Models.Account;

public class LoginWith2FAViewModel
{
    [Required(ErrorMessage = "Two factor code is required")]
    [StringLength(7, ErrorMessage = "{0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Text)]
    [Display(Name = "Authentication Code")]
    public string TwoFactorCode { get; set; } = string.Empty;

    [Display(Name = "Remember this machine")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
