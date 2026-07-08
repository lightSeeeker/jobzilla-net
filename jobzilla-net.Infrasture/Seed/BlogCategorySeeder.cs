using jobzilla_net.Core.Entities;
using jobzilla_net.Infrasture.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace jobzilla_net.Infrasture.Seed;

public static class BlogCategorySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Skip if categories already exist
        if (await context.BlogCategories.AsQueryable().AnyAsync())
            return;

        var names = new[]
        {
            "Career Advice",
            "Job Search",
            "Interview Tips",
            "Resume & CV",
            "Workplace",
            "Recruitment",
            "Industry News",
            "Remote Work",
        };

        context.BlogCategories.AddRange(names.Select(n => new BlogCategory { Name = n }));
        await context.SaveChangesAsync();
    }
}
