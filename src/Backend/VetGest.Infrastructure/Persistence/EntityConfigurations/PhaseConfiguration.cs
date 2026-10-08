using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetGest.Domain.Entities;

namespace VetGest.Infrastructure.Persistence.EntityConfigurations;

/// <summary>
/// EF Core configuration for the Phase entity.
/// Phase is reference data, NOT tenant-isolated (no OwnerId filter).
/// </summary>
public class PhaseConfiguration : IEntityTypeConfiguration<Phase>
{
    public void Configure(EntityTypeBuilder<Phase> builder)
    {
        // Table mapping
        builder.ToTable("Phases");

        // Primary key
        builder.HasKey(ph => ph.Id);
        builder.Property(ph => ph.Id)
            .ValueGeneratedNever(); // GUID provided by application

        // Properties
        builder.Property(ph => ph.Name)
            .IsRequired()
            .HasMaxLength(50); // "Initial", "Intermediate", "Growth", "Final", "Birth"

        builder.Property(ph => ph.Species)
            .IsRequired()
            .HasMaxLength(10); // "Dog" or "Cat"

        builder.Property(ph => ph.StartDayGestation)
            .IsRequired();

        builder.Property(ph => ph.EndDayGestation)
            .IsRequired();

        builder.Property(ph => ph.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ph => ph.FetalDevelopmentContent)
            .IsRequired()
            .HasMaxLength(2000); // Markdown content

        builder.Property(ph => ph.MaternalChangesContent)
            .IsRequired()
            .HasMaxLength(2000); // Markdown content

        // Shadow properties (audit only, no OwnerId for reference data)
        builder.Property<DateTime>("CreatedAt")
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes
        builder.HasIndex(new[] { "Species", "StartDayGestation", "EndDayGestation" });
        builder.HasIndex(ph => ph.Name);

        // Seed reference data (initial phases)
        SeedPhases(builder);

        // NO global query filter for reference data
    }

    private void SeedPhases(EntityTypeBuilder<Phase> builder)
    {
        // Dog phases (gestation ~63 days)
        builder.HasData(
            new Phase(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "Initial",
                "Dog",
                0, 14,
                "Fertilization and early embryo development",
                "Embryos are microscopic. Cell division begins and implantation occurs.",
                "Mother shows minimal physical changes. Appetite and behavior may remain normal."),
            new Phase(
                Guid.Parse("11111111-1111-1111-1111-111111111112"),
                "Intermediate",
                "Dog",
                15, 29,
                "Organogenesis and organ formation",
                "Organs and body systems begin forming. Fetuses grow rapidly.",
                "Mother may experience morning sickness, increased appetite, or mood changes."),
            new Phase(
                Guid.Parse("11111111-1111-1111-1111-111111111113"),
                "Growth",
                "Dog",
                30, 45,
                "Bone mineralization and maternal demand increase",
                "Fetuses develop bones, teeth, and hair. Significant size increase.",
                "Mother's abdomen visibly enlarges. Appetite and water intake increase significantly."),
            new Phase(
                Guid.Parse("11111111-1111-1111-1111-111111111114"),
                "Final",
                "Dog",
                46, 62,
                "Final maturation and pre-birth preparation",
                "Fetuses are now viable. Lungs mature and position for birth.",
                "Mother seeks nesting materials, displays restlessness, and may refuse food near due date."),
            new Phase(
                Guid.Parse("11111111-1111-1111-1111-111111111115"),
                "Birth",
                "Dog",
                63, 70,
                "Labor and delivery",
                "Fetuses are fully developed and ready for birth.",
                "Mother shows clear labor signs: contractions, nesting behavior, and discharge."));

        // Cat phases (gestation ~65 days)
        builder.HasData(
            new Phase(
                Guid.Parse("22222222-2222-2222-2222-222222222211"),
                "Initial",
                "Cat",
                0, 14,
                "Fertilization and early embryo development",
                "Embryos are microscopic. Implantation occurs in the uterine wall.",
                "Mother may show subtle behavioral changes or temporarily hide."),
            new Phase(
                Guid.Parse("22222222-2222-2222-2222-222222222212"),
                "Intermediate",
                "Cat",
                15, 30,
                "Organogenesis and major organ formation",
                "Organs develop rapidly. Embryos become recognizable kittens.",
                "Mother exhibits nesting behavior and may seek more attention."),
            new Phase(
                Guid.Parse("22222222-2222-2222-2222-222222222213"),
                "Growth",
                "Cat",
                31, 48,
                "Fetal growth and bone formation",
                "Rapid growth phase. Kittens develop their characteristic features.",
                "Mother's sides widen. May display increased vocalization and restlessness."),
            new Phase(
                Guid.Parse("22222222-2222-2222-2222-222222222214"),
                "Final",
                "Cat",
                49, 64,
                "Final maturation and birth preparation",
                "Kittens fully formed and viable. Fine-tuning of body systems.",
                "Mother seeks secure nesting spaces and may stop eating days before labor."),
            new Phase(
                Guid.Parse("22222222-2222-2222-2222-222222222215"),
                "Birth",
                "Cat",
                65, 72,
                "Labor and delivery",
                "Kittens are fully developed and ready to be born.",
                "Clear labor signs: panting, purring, tail twitching, and abdominal straining."));
    }
}
