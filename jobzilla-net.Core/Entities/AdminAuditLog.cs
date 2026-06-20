using jobzilla_net.Core.Common;

namespace jobzilla_net.Core.Entities;

public class AdminAuditLog : AuditableEntity
{
    public string AdminUserId { get; set; } = string.Empty;
    public string AdminName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime PerformedAtUtc { get; set; } = DateTime.UtcNow;
}
