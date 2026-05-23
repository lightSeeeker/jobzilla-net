using Jobzilla.Application.Common.Interfaces;
using Jobzilla.Domain.Common;
using Jobzilla.Domain.Entities;
using Jobzilla.Domain.Enums;
using Jobzilla.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Jobzilla.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<EmployerProfile> EmployerProfiles => Set<EmployerProfile>();
    public DbSet<JobPost> JobPosts => Set<JobPost>();
    public DbSet<JobCategory> JobCategories => Set<JobCategory>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<CandidateResume> CandidateResumes => Set<CandidateResume>();
    public DbSet<SavedJob> SavedJobs => Set<SavedJob>();
    public DbSet<JobAlert> JobAlerts => Set<JobAlert>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<EmployerSubscription> EmployerSubscriptions => Set<EmployerSubscription>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<ContentPage> ContentPages => Set<ContentPage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<CandidateProfile>().HasIndex(x => x.UserId).IsUnique();
        builder.Entity<EmployerProfile>().HasIndex(x => x.UserId).IsUnique();
        builder.Entity<JobCategory>().HasIndex(x => x.Name).IsUnique();
        builder.Entity<Skill>().HasIndex(x => x.Name).IsUnique();
        builder.Entity<BlogPost>().HasIndex(x => x.Slug).IsUnique();
        builder.Entity<ContentPage>().HasIndex(x => x.Key).IsUnique();

        builder.Entity<CandidateSkill>().HasKey(x => new { x.CandidateProfileId, x.SkillId });
        builder.Entity<JobPostSkill>().HasKey(x => new { x.JobPostId, x.SkillId });
        builder.Entity<SavedJob>().HasKey(x => new { x.CandidateProfileId, x.JobPostId });

        builder.Entity<JobPost>().Property(x => x.EmploymentType).HasConversion<string>();
        builder.Entity<JobPost>().Property(x => x.Status).HasConversion<string>();
        builder.Entity<JobApplication>().Property(x => x.Status).HasConversion<string>();
        builder.Entity<EmployerSubscription>().Property(x => x.Status).HasConversion<string>();
        builder.Entity<CandidateProfile>().Property(x => x.ExpectedSalary).HasPrecision(18, 2);
        builder.Entity<JobPost>().Property(x => x.MinimumSalary).HasPrecision(18, 2);
        builder.Entity<JobPost>().Property(x => x.MaximumSalary).HasPrecision(18, 2);
        builder.Entity<PaymentTransaction>().Property(x => x.Amount).HasPrecision(18, 2);
        builder.Entity<SubscriptionPlan>().Property(x => x.Price).HasPrecision(18, 2);

        builder.Entity<JobCategory>().HasData(
            new JobCategory { Id = 1, Name = "Web Development", IconCssClass = "flaticon-dashboard", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new JobCategory { Id = 2, Name = "Design", IconCssClass = "flaticon-dashboard", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new JobCategory { Id = 3, Name = "Marketing", IconCssClass = "flaticon-dashboard", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) });

        builder.Entity<EmployerProfile>().HasData(new EmployerProfile
        {
            Id = 1,
            UserId = "seed-employer",
            CompanyName = "Jobzilla Demo Company",
            Industry = "Technology",
            Location = "Doha, Qatar",
            IsVerified = true,
            CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        builder.Entity<JobPost>().HasData(
            new JobPost
            {
                Id = 1,
                EmployerProfileId = 1,
                JobCategoryId = 1,
                Title = "Senior Web Designer",
                Description = "Design and build polished web experiences for employers and candidates.",
                Location = "Doha, Qatar",
                EmploymentType = EmploymentType.FullTime,
                Status = JobStatus.Published,
                MinimumSalary = 8000,
                MaximumSalary = 12000,
                IsFeatured = true,
                CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new JobPost
            {
                Id = 2,
                EmployerProfileId = 1,
                JobCategoryId = 2,
                Title = "Product UI Designer",
                Description = "Create dashboard and job-search interfaces for a modern hiring platform.",
                Location = "Remote",
                EmploymentType = EmploymentType.Contract,
                Status = JobStatus.Published,
                MinimumSalary = 6000,
                MaximumSalary = 10000,
                CreatedAtUtc = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)
            });

        builder.Entity<SubscriptionPlan>().HasData(
            new SubscriptionPlan { Id = 1, Name = "Basic", Price = 29, DurationDays = 30, JobPostLimit = 3, CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new SubscriptionPlan { Id = 2, Name = "Professional", Price = 79, DurationDays = 30, JobPostLimit = 15, CanFeatureJobs = true, CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
