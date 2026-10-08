using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetGest.Domain.Entities;

namespace VetGest.Infrastructure.Persistence.EntityConfigurations;

/// <summary>
/// EF Core configuration for the VetConnection entity.
/// Manages the many-to-many relationship between Vets and Pregnancies via invitations.
/// </summary>
public class VetConnectionConfiguration : IEntityTypeConfiguration<VetConnection>
{
    public void Configure(EntityTypeBuilder<VetConnection> builder)
    {
        // Table mapping
        builder.ToTable("VetConnections");

        // Primary key
        builder.HasKey(vc => vc.Id);
        builder.Property(vc => vc.Id)
            .ValueGeneratedNever(); // GUID provided by application

        // Foreign key
        builder.Property(vc => vc.PregnancyId)
            .IsRequired();

        // Properties
        builder.Property(vc => vc.VetUserId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(vc => vc.TutorUserId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(vc => vc.InvitationCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(vc => vc.InvitationExpiresAt)
            .IsRequired();

        builder.Property(vc => vc.AcceptedAt)
            .IsRequired(false);

        builder.Property(vc => vc.Status)
            .IsRequired()
            .HasMaxLength(50) // "Pending", "Active", "Revoked"
            .HasDefaultValue("Pending");

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
        builder.HasIndex(vc => vc.PregnancyId);
        builder.HasIndex(vc => vc.InvitationCode).IsUnique();
        builder.HasIndex(vc => vc.VetUserId);
        builder.HasIndex(vc => vc.TutorUserId);
        builder.HasIndex(vc => vc.Status);
        builder.HasIndex(new[] { "OwnerId" });
        builder.HasIndex(new[] { "PregnancyId", "Status" });
        builder.HasIndex(new[] { "VetUserId", "Status" });

        // Relationships
        builder.HasOne(vc => vc.Pregnancy)
            .WithMany(pr => pr.VetConnections)
            .HasForeignKey(vc => vc.PregnancyId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // No direct navigation to Users (they're in Identity tables)
        // Authorization layer will validate VetUserId and TutorUserId exist

        // Global query filter (applied in DbContext)
        builder.HasQueryFilter(p => EF.Property<string>(p, "OwnerId") == EF.Property<string>(p, "OwnerId"));
    }
}
