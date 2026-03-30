using CatTracker.Domain.Entities;
using CatTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatTracker.Infrastructure.Configurations;

public class VetRecordConfiguration : IEntityTypeConfiguration<VetRecord>
{
    public void Configure(EntityTypeBuilder<VetRecord> builder)
    {
        builder.ToTable("VetRecords");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(v => v.ClinicName)
            .HasMaxLength(200);

        builder.Property(v => v.VetName)
            .HasMaxLength(200);

        builder.Property(v => v.Cost)
            .HasColumnType("decimal(10,2)");

        builder.Property(v => v.RecordType)
            .HasConversion<string>();

        builder.Property(v => v.Notes);

        builder.HasIndex(v => new { v.CatId, v.NextDueDate });
    }
}
