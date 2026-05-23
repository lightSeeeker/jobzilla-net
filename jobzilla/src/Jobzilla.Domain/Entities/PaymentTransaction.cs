using Jobzilla.Domain.Common;

namespace Jobzilla.Domain.Entities;

public class PaymentTransaction : AuditableEntity
{
    public int EmployerSubscriptionId { get; set; }
    public EmployerSubscription? EmployerSubscription { get; set; }
    public string Reference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = "Pending";
    public DateTime TransactionDateUtc { get; set; } = DateTime.UtcNow;
}
