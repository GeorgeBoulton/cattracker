using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CatTracker.Infrastructure.Data;

public class CatTrackerDbContextFactory : IDesignTimeDbContextFactory<CatTrackerDbContext>
{
    public CatTrackerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatTrackerDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=cattracker;Username=cattracker;Password=cattracker");
        return new CatTrackerDbContext(optionsBuilder.Options);
    }
}
