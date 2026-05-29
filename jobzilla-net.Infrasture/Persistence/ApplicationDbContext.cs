using jobzilla_net.Application.Common.Interfaces;
using jobzilla_net.Core.Common;
using jobzilla_net.Core.Entities;
using jobzilla_net.Core.Enums;
using jobzilla_net.Infrasture.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace jobzilla_net.Infrasture.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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
    public DbSet<CandidateExperience> CandidateExperiences => Set<CandidateExperience>();
    public DbSet<CandidateEducation> CandidateEducations => Set<CandidateEducation>();
    public DbSet<CandidateCertification> CandidateCertifications => Set<CandidateCertification>();
    public DbSet<CandidateProject> CandidateProjects => Set<CandidateProject>();
    public DbSet<CandidateSocialLink> CandidateSocialLinks => Set<CandidateSocialLink>();
    public DbSet<CandidateReference> CandidateReferences => Set<CandidateReference>();
    public DbSet<ResumeTemplate> ResumeTemplates => Set<ResumeTemplate>();
    public DbSet<SavedJob> SavedJobs => Set<SavedJob>();
    public DbSet<CandidateSkill> CandidateSkills => Set<CandidateSkill>();
    public DbSet<JobPostSkill> JobPostSkills => Set<JobPostSkill>();
    public DbSet<JobAlert> JobAlerts => Set<JobAlert>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<EmployerSubscription> EmployerSubscriptions => Set<EmployerSubscription>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<BlogCategory> BlogCategories => Set<BlogCategory>();
    public DbSet<ContentPage> ContentPages => Set<ContentPage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureIndexes(builder);
        ConfigureKeys(builder);
        ConfigureConversions(builder);
        ConfigurePrecision(builder);
        SeedData(builder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private static void ConfigureIndexes(ModelBuilder builder)
    {
        builder.Entity<CandidateProfile>().HasIndex(x => x.UserId).IsUnique();
        builder.Entity<EmployerProfile>().HasIndex(x => x.UserId).IsUnique();
        builder.Entity<JobCategory>().HasIndex(x => x.Name).IsUnique();
        builder.Entity<Skill>().HasIndex(x => x.Name).IsUnique();
        builder.Entity<BlogPost>().HasIndex(x => x.Slug).IsUnique();
        builder.Entity<BlogCategory>().HasIndex(x => x.Name).IsUnique();
        builder.Entity<ContentPage>().HasIndex(x => x.Key).IsUnique();
    }

    private static void ConfigureKeys(ModelBuilder builder)
    {
        builder.Entity<CandidateSkill>().HasKey(x => new { x.CandidateProfileId, x.SkillId });
        builder.Entity<JobPostSkill>().HasKey(x => new { x.JobPostId, x.SkillId });
        builder.Entity<SavedJob>().HasKey(x => new { x.CandidateProfileId, x.JobPostId });
    }

    private static void ConfigureConversions(ModelBuilder builder)
    {
        builder.Entity<JobPost>().Property(x => x.EmploymentType).HasConversion<string>();
        builder.Entity<JobPost>().Property(x => x.Status).HasConversion<string>();
        builder.Entity<JobApplication>().Property(x => x.Status).HasConversion<string>();
        builder.Entity<EmployerSubscription>().Property(x => x.Status).HasConversion<string>();
    }

    private static void ConfigurePrecision(ModelBuilder builder)
    {
        builder.Entity<CandidateProfile>().Property(x => x.ExpectedSalary).HasPrecision(18, 2);
        builder.Entity<JobPost>().Property(x => x.MinimumSalary).HasPrecision(18, 2);
        builder.Entity<JobPost>().Property(x => x.MaximumSalary).HasPrecision(18, 2);
        builder.Entity<PaymentTransaction>().Property(x => x.Amount).HasPrecision(18, 2);
        builder.Entity<SubscriptionPlan>().Property(x => x.Price).HasPrecision(18, 2);
    }

    private static void SeedData(ModelBuilder builder)
    {
        builder.Entity<JobCategory>().HasData(
            new JobCategory
            {
                Id = 1,
                Name = "Web Development",
                IconCssClass = "flaticon-dashboard",
                Description = "Frontend, backend, and full-stack web roles.",
                CreatedAtUtc = SeedDate
            },
            new JobCategory
            {
                Id = 2,
                Name = "Design",
                IconCssClass = "flaticon-design",
                Description = "UI, UX, product, and visual design roles.",
                CreatedAtUtc = SeedDate
            },
            new JobCategory
            {
                Id = 3,
                Name = "Marketing",
                IconCssClass = "flaticon-customer-service",
                Description = "Growth, content, SEO, and digital marketing roles.",
                CreatedAtUtc = SeedDate
            });

        builder.Entity<SubscriptionPlan>().HasData(
            new SubscriptionPlan
            {
                Id = 1,
                Name = "Basic",
                Price = 29m,
                DurationDays = 30,
                JobPostLimit = 3,
                CanFeatureJobs = false,
                CreatedAtUtc = SeedDate
            },
            new SubscriptionPlan
            {
                Id = 2,
                Name = "Professional",
                Price = 79m,
                DurationDays = 30,
                JobPostLimit = 15,
                CanFeatureJobs = true,
                CreatedAtUtc = SeedDate
            });

        builder.Entity<EmployerProfile>().HasData(new EmployerProfile
        {
            Id = 1,
            UserId = "seed-employer",
            CompanyName = "Jobzilla Demo Company",
            Industry = "Technology",
            WebsiteUrl = "https://jobzilla.local",
            Email = "employer@jobzilla.local",
            Location = "Doha, Qatar",
            Description = "Seed employer profile for local development.",
            IsVerified = true,
            CreatedAtUtc = SeedDate
        });

        builder.Entity<JobPost>().HasData(
            new JobPost
            {
                Id = 1,
                EmployerProfileId = 1,
                JobCategoryId = 1,
                Title = "Senior Web Developer",
                Slug = "senior-web-developer",
                Description = "Build and maintain high-quality web applications.",
                Requirements = "Strong ASP.NET Core and JavaScript experience.",
                Responsibilities = "Deliver features, review code, and improve platform quality.",
                Location = "Doha, Qatar",
                EmploymentType = EmploymentType.FullTime,
                Status = JobStatus.Published,
                MinimumSalary = 8000m,
                MaximumSalary = 12000m,
                ExpiresAtUtc = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                IsFeatured = true,
                CreatedAtUtc = SeedDate
            },
            new JobPost
            {
                Id = 2,
                EmployerProfileId = 1,
                JobCategoryId = 2,
                Title = "Product UI Designer",
                Slug = "product-ui-designer",
                Description = "Create clean and usable hiring platform interfaces.",
                Requirements = "Portfolio showing UI systems and responsive web design.",
                Responsibilities = "Design flows, prototypes, and production-ready UI specs.",
                Location = "Remote",
                EmploymentType = EmploymentType.Contract,
                Status = JobStatus.Published,
                MinimumSalary = 6000m,
                MaximumSalary = 10000m,
                ExpiresAtUtc = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                IsFeatured = false,
                CreatedAtUtc = SeedDate
            });

        builder.Entity<ResumeTemplate>().HasData(
            new ResumeTemplate
            {
                Id = 1,
                Name = "Standard Professional",
                Description = "A clean, professional template suitable for all industries.",
                TemplateFilePath = "Standard", // Corresponds to Standard.cshtml
                IsActive = true,
                CreatedAtUtc = SeedDate
            },
            new ResumeTemplate
            {
                Id = 2,
                Name = "Modern Creative",
                Description = "A sleek, modern design with vibrant accents for creative roles.",
                TemplateFilePath = "Modern", // Corresponds to Modern.cshtml
                IsActive = true,
                CreatedAtUtc = SeedDate
            });
    }
}
