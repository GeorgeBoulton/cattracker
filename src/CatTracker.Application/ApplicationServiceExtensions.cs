using Microsoft.Extensions.DependencyInjection;

namespace CatTracker.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Application services will be registered here in Phase 4
        return services;
    }
}
