using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace jobzilla_net.Infrasture.Persistence;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? ReadDefaultConnectionString()
            ?? "Server=(localdb)\\mssqllocaldb;Database=JobzillaDb;Trusted_Connection=True;MultipleActiveResultSets=true";

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static string? ReadDefaultConnectionString()
    {
        var basePath = Directory.GetCurrentDirectory();
        var candidatePaths = new[]
        {
            Path.Combine(basePath, "appsettings.json"),
            Path.GetFullPath(Path.Combine(basePath, "..", "jobzilla-net", "appsettings.json"))
        };

        foreach (var path in candidatePaths)
        {
            if (!File.Exists(path))
            {
                continue;
            }

            using var stream = File.OpenRead(path);
            using var document = JsonDocument.Parse(stream);
            if (document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings)
                && connectionStrings.TryGetProperty("DefaultConnection", out var defaultConnection))
            {
                return defaultConnection.GetString();
            }
        }

        return null;
    }
}
