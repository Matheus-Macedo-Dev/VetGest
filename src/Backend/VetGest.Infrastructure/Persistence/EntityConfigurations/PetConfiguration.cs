using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetGest.Domain.Entities;

namespace VetGest.Infrastructure.Persistence.EntityConfigurations;

/// <summary>
/// EF Core configuration for the Pet entity.
/// Sets up shadow properties (OwnerId, CreatedAt, ModifiedAt), indexes, and global query filters.
/// </summary>
public class PetConfiguration : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        // Table mapping
        builder.ToTable("Pets");

        // Primary key
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .ValueGeneratedNever(); // GUID provided by application

        // Properties
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Species)
            .IsRequired()
            .HasMaxLength(10); // "Dog" or "Cat"

        builder.Property(p => p.Breed)
            .HasMaxLength(100);

        builder.Property(p => p.DateOfBirth)
            .IsRequired(false);

        builder.Property(p => p.CurrentWeight)
            .HasPrecision(5, 2); // Up to 999.99 kg

        builder.Property(p => p.PhotoUrl)
            .HasMaxLength(500);

        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);

        // Shadow properties (not exposed as CLR properties)
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
        builder.HasIndex(new[] { "OwnerId", "IsActive" });
        builder.HasIndex(p => p.Name);

        // Relationships
        builder.HasMany(p => p.Pregnancies)
            .WithOne(pr => pr.Pet)
            .HasForeignKey(pr => pr.PetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Global query filter (applied in DbContext)
        builder.HasQueryFilter(p => EF.Property<string>(p, "OwnerId") == EF.Property<string>(p, "OwnerId"));
    }
}
