using CatTracker.Domain.Entities;
using CatTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatTracker.Infrastructure.Configurations;

public class FeedingLogConfiguration : IEntityTypeConfiguration<FeedingLog>
{
    public void Configure(EntityTypeBuilder<FeedingLog> builder)
    {
        builder.ToTable("FeedingLogs");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.FoodBrand)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.AmountGrams)
            .HasColumnType("decimal(10,2)");

        builder.Property(f => f.FoodType)
            .HasConversion<string>();

        builder.Property(f => f.Notes)
            .HasMaxLength(500);

        builder.HasIndex(f => f.CatId);
    }
}
