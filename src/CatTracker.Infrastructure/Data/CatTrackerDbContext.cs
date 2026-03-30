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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(CatTrackerDbContext).Assembly);
    }
}
