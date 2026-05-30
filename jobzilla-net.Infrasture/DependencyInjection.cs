using jobzilla_net.Application.Blogs;
using jobzilla_net.Application.Candidates;
using jobzilla_net.Application.Common.Interfaces;
using jobzilla_net.Application.Employers;
using jobzilla_net.Application.Jobs;
using jobzilla_net.Application.Resumes.Interfaces;
using jobzilla_net.Infrasture.Identity;
using jobzilla_net.Infrasture.Persistence;
using jobzilla_net.Infrasture.Services;
using jobzilla_net.Infrasture.Services.Resumes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace jobzilla_net.Infrasture;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;

                // Account lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Explicitly configure the authentication cookie paths.
        // This prevents redirect loops and ensures unauthorized access sends
        // users to the correct login page rather than throwing a 404.
        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";

            // Prevent the cookie from expiring during an active session
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
        });

        // Bind IApplicationDbContext to the already-registered ApplicationDbContext
        // — avoids a second DbContext lifetime and keeps a single scoped instance.
        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        // ── Application services ──────────────────────────────────────────────
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<ICandidateService, CandidateService>();
        services.AddScoped<IEmployerService, EmployerService>();
        services.AddScoped<IBlogService, BlogService>();
        services.AddScoped<ICandidateDashboardService, CandidateDashboardService>();
        services.AddScoped<IEmployerDashboardService, EmployerDashboardService>();
        services.AddScoped<jobzilla_net.Application.Chat.IChatService, jobzilla_net.Infrasture.Services.Chat.ChatService>();

        // ── Resume Parsing Pipeline ──────────────────────────────────────────
        services.AddScoped<IResumeDataSyncService, ResumeDataSyncService>();
        services.AddScoped<IResumeParsingOrchestrator, ResumeParsingOrchestrator>();
        services.AddScoped<IResumeBuilderService, ResumeBuilderService>();
        services.AddScoped<ITemplateRenderer, RazorTemplateRenderer>();
        services.AddScoped<IPdfGenerator, SelectPdfGenerator>();
        services.AddScoped<IResumeExportService, ResumeExportService>();

        return services;
    }
}
