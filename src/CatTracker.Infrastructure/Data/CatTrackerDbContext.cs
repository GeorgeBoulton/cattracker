using CatTracker.Domain.Entities;
using CatTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CatTracker.Infrastructure.Data;

public class CatTrackerDbContext : IdentityDbContext<ApplicationUser>
{
    public CatTrackerDbContext(DbContextOptions<CatTrackerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cat> Cats => Set<Cat>();
    public DbSet<FeedingLog> FeedingLogs => Set<FeedingLog>();
    public DbSet<FoodStock> FoodStocks => Set<FoodStock>();
    public DbSet<LitterLog> LitterLogs => Set<LitterLog>();
    public DbSet<WaterLog> WaterLogs => Set<WaterLog>();
    public DbSet<VetRecord> VetRecords => Set<VetRecord>();
    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(CatTrackerDbContext).Assembly);
    }
}
