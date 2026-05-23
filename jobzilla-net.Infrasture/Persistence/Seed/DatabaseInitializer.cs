using jobzilla_net.Infrasture.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace jobzilla_net.Infrasture.Seed;

public static class DatabaseInitializer
{
    /// <summary>
    /// Applies any pending EF Core migrations at application startup.
    /// Safe to call on every startup — no-ops if schema is already current.
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.MigrateAsync();
    }
}
