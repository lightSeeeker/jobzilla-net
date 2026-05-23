using Microsoft.AspNetCore.Identity;

namespace Jobzilla.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public string? UserType { get; set; }
    public bool IsActive { get; set; } = true;
}
