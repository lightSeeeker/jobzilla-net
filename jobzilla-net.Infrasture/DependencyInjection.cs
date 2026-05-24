using jobzilla_net.Application.Blogs;
using jobzilla_net.Application.Candidates;
using jobzilla_net.Application.Common.Interfaces;
using jobzilla_net.Application.Employers;
using jobzilla_net.Application.Jobs;
using jobzilla_net.Infrasture.Identity;
using jobzilla_net.Infrasture.Persistence;
using jobzilla_net.Infrasture.Services;
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

        return services;
    }
}
