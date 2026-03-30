using CatTracker.Domain.Entities;
using CatTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatTracker.Infrastructure.Configurations;

public class LitterLogConfiguration : IEntityTypeConfiguration<LitterLog>
{
    public void Configure(EntityTypeBuilder<LitterLog> builder)
    {
        builder.ToTable("LitterLogs");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.EntryType)
            .HasConversion<string>();

        builder.Property(l => l.Notes)
            .HasMaxLength(500);

        builder.HasIndex(l => l.CatId);
    }
}
