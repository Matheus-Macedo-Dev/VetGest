using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetGest.Domain.Entities;

namespace VetGest.Infrastructure.Persistence.EntityConfigurations;

/// <summary>
/// EF Core configuration for the AlertRule entity.
/// AlertRule is reference data, NOT tenant-isolated (no OwnerId filter).
/// </summary>
public class AlertRuleConfiguration : IEntityTypeConfiguration<AlertRule>
{
    public void Configure(EntityTypeBuilder<AlertRule> builder)
    {
        // Table mapping
        builder.ToTable("AlertRules");

        // Primary key
        builder.HasKey(ar => ar.Id);
        builder.Property(ar => ar.Id)
            .ValueGeneratedNever(); // GUID provided by application

        // Properties
        builder.Property(ar => ar.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ar => ar.Species)
            .IsRequired()
            .HasMaxLength(10); // "Dog" or "Cat"

        builder.Property(ar => ar.Severity)
            .IsRequired()
            .HasMaxLength(50) // "Normal", "Attention", "Emergency"
            .HasDefaultValue("Normal");

        builder.Property(ar => ar.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ar => ar.Conditions)
            .IsRequired()
            .HasMaxLength(1000); // JSON or comma-separated conditions

        builder.Property(ar => ar.IsActive)
            .HasDefaultValue(true);

        // Shadow properties (audit only, no OwnerId for reference data)
        builder.Property<DateTime>("CreatedAt")
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("timezone('utc', now())");

        // Indexes
        // Composite unique index: (Species, Name) allows same names for different species
        builder.HasIndex(new[] { "Species", "Name" }).IsUnique();
        builder.HasIndex(new[] { "Species", "Severity" });
        builder.HasIndex(ar => ar.IsActive);

        // Seed reference data (common alert rules)
        SeedAlertRules(builder);

        // NO global query filter for reference data
    }

    private void SeedAlertRules(EntityTypeBuilder<AlertRule> builder)
    {
        // Common alert rules for dogs and cats
        builder.HasData(
            // Temperature alerts
            new AlertRule(
                Guid.Parse("33333333-3333-3333-3333-333333333311"),
                "High Temperature",
                "Dog",
                "Attention",
                "Mother's temperature is elevated, which may indicate infection or stress.",
                "{\"temperature_min\": 39.5, \"temperature_max\": 40.5}"),
            new AlertRule(
                Guid.Parse("33333333-3333-3333-3333-333333333312"),
                "Very High Temperature",
                "Dog",
                "Emergency",
                "Mother's temperature is dangerously high. Seek veterinary care immediately.",
                "{\"temperature_min\": 40.5}"),
            new AlertRule(
                Guid.Parse("33333333-3333-3333-3333-333333333313"),
                "Low Temperature",
                "Dog",
                "Attention",
                "Mother's temperature is below normal, which may indicate illness or stress.",
                "{\"temperature_max\": 37.5}"),

            // Appetite alerts
            new AlertRule(
                Guid.Parse("33333333-3333-3333-3333-333333333314"),
                "Decreased Appetite",
                "Dog",
                "Attention",
                "Mother shows reduced interest in food, which may be normal near birth but needs monitoring.",
                "{\"appetite\": \"decreased\"}"),
            new AlertRule(
                Guid.Parse("33333333-3333-3333-3333-333333333315"),
                "No Food Intake",
                "Dog",
                "Emergency",
                "Mother refuses all food. Consult a veterinarian if this persists.",
                "{\"appetite\": \"none\"}"),

            // Symptom alerts
            new AlertRule(
                Guid.Parse("33333333-3333-3333-3333-333333333316"),
                "Abnormal Discharge",
                "Dog",
                "Emergency",
                "Abnormal discharge may indicate infection or complications. Seek immediate veterinary care.",
                "discharge,bleeding"),
            new AlertRule(
                Guid.Parse("33333333-3333-3333-3333-333333333317"),
                "Severe Lethargy",
                "Dog",
                "Emergency",
                "Mother is unusually inactive or unresponsive. This requires immediate veterinary evaluation.",
                "lethargy,unresponsive"),

            // Cat-specific alerts
            new AlertRule(
                Guid.Parse("33333333-3333-3333-3333-333333333321"),
                "High Temperature",
                "Cat",
                "Attention",
                "Mother's temperature is elevated, which may indicate infection or stress.",
                "{\"temperature_min\": 39.0, \"temperature_max\": 39.8}"),
            new AlertRule(
                Guid.Parse("33333333-3333-3333-3333-333333333322"),
                "Very High Temperature",
                "Cat",
                "Emergency",
                "Mother's temperature is dangerously high. Seek veterinary care immediately.",
                "{\"temperature_min\": 39.8}"),
            new AlertRule(
                Guid.Parse("33333333-3333-3333-3333-333333333323"),
                "Low Temperature",
                "Cat",
                "Attention",
                "Mother's temperature is below normal, which may indicate illness or stress.",
                "{\"temperature_max\": 37.5}"),
            new AlertRule(
                Guid.Parse("33333333-3333-3333-3333-333333333324"),
                "Decreased Appetite",
                "Cat",
                "Attention",
                "Mother shows reduced interest in food, which may be normal near birth but needs monitoring.",
                "{\"appetite\": \"decreased\"}"),
            new AlertRule(
                Guid.Parse("33333333-3333-3333-3333-333333333325"),
                "Abnormal Discharge",
                "Cat",
                "Emergency",
                "Abnormal discharge may indicate infection or complications. Seek immediate veterinary care.",
                "discharge,bleeding"),
            new AlertRule(
                Guid.Parse("33333333-3333-3333-3333-333333333326"),
                "Severe Lethargy",
                "Cat",
                "Emergency",
                "Mother is unusually inactive or unresponsive. This requires immediate veterinary evaluation.",
                "lethargy,unresponsive"));
    }
}
