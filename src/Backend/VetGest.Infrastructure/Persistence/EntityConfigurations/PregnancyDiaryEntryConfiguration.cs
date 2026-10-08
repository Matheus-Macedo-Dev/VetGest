using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetGest.Domain.Entities;

namespace VetGest.Infrastructure.Persistence.EntityConfigurations;

/// <summary>
/// EF Core configuration for the PregnancyDiaryEntry entity.
/// Sets up shadow properties, indexes, and global query filters.
/// </summary>
public class PregnancyDiaryEntryConfiguration : IEntityTypeConfiguration<PregnancyDiaryEntry>
{
    public void Configure(EntityTypeBuilder<PregnancyDiaryEntry> builder)
    {
        // Table mapping
        builder.ToTable("PregnancyDiaryEntries");

        // Primary key
        builder.HasKey(de => de.Id);
        builder.Property(de => de.Id)
            .ValueGeneratedNever(); // GUID provided by application

        // Foreign key
        builder.Property(de => de.PregnancyId)
            .IsRequired();

        // Properties
        builder.Property(de => de.EntryDate)
            .IsRequired()
            .HasConversion(
                value => value,
                value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

        builder.Property(de => de.Weight)
            .IsRequired(false)
            .HasPrecision(5, 2); // Up to 999.99 kg

        builder.Property(de => de.Appetite)
            .IsRequired(false)
            .HasMaxLength(50); // "Normal", "Decreased", "Increased"

        builder.Property(de => de.Behavior)
            .IsRequired(false)
            .HasMaxLength(1000);

        builder.Property(de => de.Temperature)
            .IsRequired(false)
            .HasPrecision(4, 1); // Up to 39.5°C

        builder.Property(de => de.SymptomsList)
            .IsRequired(false)
            .HasMaxLength(500); // Comma-separated symptoms

        builder.Property(de => de.PhotoUrls)
            .IsRequired(false)
            .HasMaxLength(2000); // Comma-separated URLs

        builder.Property(de => de.Notes)
            .IsRequired(false)
            .HasMaxLength(1000);

        // Shadow properties
        builder.Property<string>("OwnerId")
            .IsRequired()
            .HasMaxLength(128);

        builder.Property<DateTime>("CreatedAt")
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property<DateTime>("ModifiedAt")
            .HasDefaultValueSql("timezone('utc', now())");

        // Indexes
        builder.HasIndex(new[] { "OwnerId" });
        builder.HasIndex(de => de.PregnancyId);
        builder.HasIndex(de => de.EntryDate);
        builder.HasIndex(new[] { "PregnancyId", "EntryDate" }).IsUnique();
        builder.HasIndex(de => de.Temperature);

        // Relationships
        builder.HasOne(de => de.Pregnancy)
            .WithMany(pr => pr.DiaryEntries)
            .HasForeignKey(de => de.PregnancyId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // Do not rely on a tautological filter for tenant isolation. Repository queries
        // must include the authenticated owner and related pregnancy explicitly.
    }
}
