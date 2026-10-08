using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetGest.Domain.Entities;

namespace VetGest.Infrastructure.Persistence.EntityConfigurations;

/// <summary>
/// EF Core configuration for the Pregnancy entity.
/// Sets up shadow properties, indexes, relationships, and global query filters.
/// </summary>
public class PregnancyConfiguration : IEntityTypeConfiguration<Pregnancy>
{
    public void Configure(EntityTypeBuilder<Pregnancy> builder)
    {
        // Table mapping
        builder.ToTable("Pregnancies");

        // Primary key
        builder.HasKey(pr => pr.Id);
        builder.Property(pr => pr.Id)
            .ValueGeneratedNever(); // GUID provided by application

        // Foreign key
        builder.Property(pr => pr.PetId)
            .IsRequired();

        builder.Property(pr => pr.CurrentPhaseId)
            .IsRequired(false);

        // Properties
        builder.Property(pr => pr.MatingDate)
            .IsRequired(false)
            .HasConversion(
                value => value,
                value => value.HasValue
                    ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                    : (DateTime?)null);

        builder.Property(pr => pr.OvulationDate)
            .IsRequired(false)
            .HasConversion(
                value => value,
                value => value.HasValue
                    ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                    : (DateTime?)null);

        builder.Property(pr => pr.ConfirmedAt)
            .IsRequired(false)
            .HasConversion(
                value => value,
                value => value.HasValue
                    ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                    : (DateTime?)null);

        builder.Property(pr => pr.EstimatedDueDate)
            .IsRequired()
            .HasConversion(
                value => value,
                value => DateTime.SpecifyKind(value, DateTimeKind.Utc)); // Always required

        builder.Property(pr => pr.Status)
            .IsRequired()
            .HasMaxLength(50) // "Pending", "Active", "Completed", "Cancelled"
            .HasDefaultValue("Pending");

        builder.Property(pr => pr.Notes)
            .HasMaxLength(1000);

        // Shadow properties
        builder.Property<string>("OwnerId")
            .IsRequired()
            .HasMaxLength(128);

        builder.Property<DateTime>("CreatedAt")
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property<DateTime>("ModifiedAt")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(new[] { "OwnerId" });
        builder.HasIndex(pr => pr.PetId);
        builder.HasIndex(pr => pr.EstimatedDueDate);
        builder.HasIndex(pr => pr.Status);
        builder.HasIndex(new[] { "PetId", "Status" });

        // Relationships
        builder.HasOne(pr => pr.Pet)
            .WithMany(p => p.Pregnancies)
            .HasForeignKey(pr => pr.PetId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne(pr => pr.CurrentPhase)
            .WithMany()
            .HasForeignKey(pr => pr.CurrentPhaseId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasMany(pr => pr.DiaryEntries)
            .WithOne(de => de.Pregnancy)
            .HasForeignKey(de => de.PregnancyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(pr => pr.VetConnections)
            .WithOne(vc => vc.Pregnancy)
            .HasForeignKey(vc => vc.PregnancyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Owner predicates are applied explicitly by every repository query.
    }
}
