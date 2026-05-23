using jobzilla_net.Core.Common;
using jobzilla_net.Core.Enums;

namespace jobzilla_net.Core.Entities;

public class EmployerSubscription : AuditableEntity
{
    public int EmployerProfileId { get; set; }
    public EmployerProfile? EmployerProfile { get; set; }
    public int SubscriptionPlanId { get; set; }
    public SubscriptionPlan? SubscriptionPlan { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.PendingPayment;
    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }

    public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
}
