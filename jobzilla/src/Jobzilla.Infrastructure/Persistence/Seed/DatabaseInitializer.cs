using Jobzilla.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jobzilla.Infrastructure.Persistence.Seed;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DatabaseInitializer));
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("SQL Server database migrated successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to migrate SQL Server database. Verify LocalDB/SQL Server is available and the connection string is valid.");
            throw;
        }
    }
}
