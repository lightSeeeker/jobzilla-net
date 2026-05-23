using Jobzilla.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jobzilla.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<CandidateProfile> CandidateProfiles { get; }
    DbSet<EmployerProfile> EmployerProfiles { get; }
    DbSet<JobPost> JobPosts { get; }
    DbSet<JobCategory> JobCategories { get; }
    DbSet<Skill> Skills { get; }
    DbSet<JobApplication> JobApplications { get; }
    DbSet<CandidateResume> CandidateResumes { get; }
    DbSet<SavedJob> SavedJobs { get; }
    DbSet<JobAlert> JobAlerts { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<EmployerSubscription> EmployerSubscriptions { get; }
    DbSet<PaymentTransaction> PaymentTransactions { get; }
    DbSet<Conversation> Conversations { get; }
    DbSet<Message> Messages { get; }
    DbSet<BlogPost> BlogPosts { get; }
    DbSet<ContentPage> ContentPages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
