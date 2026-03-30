using CatTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatTracker.Infrastructure.Configurations;

public class WaterLogConfiguration : IEntityTypeConfiguration<WaterLog>
{
    public void Configure(EntityTypeBuilder<WaterLog> builder)
    {
        builder.ToTable("WaterLogs");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Notes)
            .HasMaxLength(500);

        builder.HasIndex(w => new { w.CatId, w.CleanedAt });
    }
}
