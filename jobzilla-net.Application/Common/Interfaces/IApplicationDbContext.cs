using jobzilla_net.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace jobzilla_net.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<CandidateProfile> CandidateProfiles { get; }
    DbSet<EmployerProfile> EmployerProfiles { get; }
    DbSet<JobPost> JobPosts { get; }
    DbSet<JobCategory> JobCategories { get; }
    DbSet<Skill> Skills { get; }
    DbSet<JobApplication> JobApplications { get; }
    DbSet<CandidateResume> CandidateResumes { get; }
    DbSet<CandidateExperience> CandidateExperiences { get; }
    DbSet<CandidateEducation> CandidateEducations { get; }
    DbSet<CandidateCertification> CandidateCertifications { get; }
    DbSet<CandidateProject> CandidateProjects { get; }
    DbSet<CandidateSocialLink> CandidateSocialLinks { get; }
    DbSet<CandidateReference> CandidateReferences { get; }
    DbSet<ResumeTemplate> ResumeTemplates { get; }
    DbSet<SavedJob> SavedJobs { get; }
    DbSet<CandidateSkill> CandidateSkills { get; }
    DbSet<JobPostSkill> JobPostSkills { get; }
    DbSet<JobAlert> JobAlerts { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<EmployerSubscription> EmployerSubscriptions { get; }
    DbSet<PaymentTransaction> PaymentTransactions { get; }
    DbSet<Conversation> Conversations { get; }
    DbSet<Message> Messages { get; }
    DbSet<BlogPost> BlogPosts { get; }
    DbSet<BlogCategory> BlogCategories { get; }
    DbSet<ContentPage> ContentPages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
