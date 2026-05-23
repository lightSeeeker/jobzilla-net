using Jobzilla.Domain.Common;

namespace Jobzilla.Domain.Entities;

public class EmployerProfile : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public string? LogoPath { get; set; }
    public string? BannerPath { get; set; }
    public bool IsVerified { get; set; }
    public ICollection<JobPost> JobPosts { get; set; } = new List<JobPost>();
    public ICollection<EmployerSubscription> Subscriptions { get; set; } = new List<EmployerSubscription>();
}
