using CatTracker.Domain.Interfaces;
using CatTracker.Infrastructure.Data;
using CatTracker.Infrastructure.Identity;
using CatTracker.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatTracker.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CatTrackerDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<CatTrackerDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<ICatRepository, CatRepository>();
        services.AddScoped<IFeedingLogRepository, FeedingLogRepository>();
        services.AddScoped<IFoodStockRepository, FoodStockRepository>();

        return services;
    }
}
