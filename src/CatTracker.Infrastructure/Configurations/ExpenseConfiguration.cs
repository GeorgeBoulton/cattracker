using CatTracker.Domain.Entities;
using CatTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatTracker.Infrastructure.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses", t => t.HasCheckConstraint("CK_Expenses_Amount_Positive", "\"Amount\" > 0"));

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(10,2)");

        builder.Property(e => e.Category)
            .HasConversion<string>();

        builder.Property(e => e.Notes);

        builder.HasIndex(e => new { e.CatId, e.Date });
    }
}
