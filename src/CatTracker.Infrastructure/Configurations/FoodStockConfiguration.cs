using CatTracker.Domain.Entities;
using CatTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatTracker.Infrastructure.Configurations;

public class FoodStockConfiguration : IEntityTypeConfiguration<FoodStock>
{
    public void Configure(EntityTypeBuilder<FoodStock> builder)
    {
        builder.ToTable("FoodStocks");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.FoodBrand)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.QuantityGrams)
            .HasColumnType("decimal(10,2)");

        builder.Property(f => f.DailyUsageGrams)
            .HasColumnType("decimal(10,2)");

        builder.Property(f => f.FoodType)
            .HasConversion<string>();

        builder.HasIndex(f => f.CatId);
    }
}
