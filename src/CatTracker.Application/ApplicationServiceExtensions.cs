using CatTracker.Application.Interfaces;
using CatTracker.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CatTracker.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICatService, CatService>();

        return services;
    }
}
