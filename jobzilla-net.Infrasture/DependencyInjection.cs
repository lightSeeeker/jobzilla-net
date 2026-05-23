using jobzilla_net.Application.Common.Interfaces;
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
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Bind IApplicationDbContext to the already-registered ApplicationDbContext
        // — avoids a second DbContext lifetime and keeps a single scoped instance.
        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        // ── Application services ──────────────────────────────────────────────
        services.AddScoped<IJobService, JobService>();

        return services;
    }
}
