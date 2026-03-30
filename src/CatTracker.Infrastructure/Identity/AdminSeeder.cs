using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CatTracker.Infrastructure.Identity;

public class AdminSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminSeeder> _logger;

    public AdminSeeder(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger<AdminSeeder> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var email = _configuration["Seed:AdminEmail"];
        var password = _configuration["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogInformation("Admin seeding skipped: Seed:AdminEmail or Seed:AdminPassword is not configured.");
            return;
        }

        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            _logger.LogInformation("Admin user '{Email}' already exists. Skipping seed.", email);
            return;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to seed admin user '{email}': {errors}");
        }

        _logger.LogInformation("Admin user '{Email}' created successfully.", email);
    }

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
        await seeder.SeedAsync();
    }
}
