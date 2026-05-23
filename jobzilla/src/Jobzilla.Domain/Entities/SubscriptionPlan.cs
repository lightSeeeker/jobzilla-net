using Jobzilla.Domain.Common;

namespace Jobzilla.Domain.Entities;

public class SubscriptionPlan : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
    public int JobPostLimit { get; set; }
    public bool CanFeatureJobs { get; set; }
    public ICollection<EmployerSubscription> EmployerSubscriptions { get; set; } = new List<EmployerSubscription>();
}
