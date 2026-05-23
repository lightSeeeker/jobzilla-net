using Jobzilla.Application.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace Jobzilla.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IJobService, JobService>();
        return services;
    }
}
