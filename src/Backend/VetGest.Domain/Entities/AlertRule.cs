using VetGest.Domain.ValueObjects;

namespace VetGest.Domain.Entities;

/// <summary>
/// AlertRule entity. Defines rules for monitoring pregnancy and triggering alerts.
/// This is shared reference data, NOT owned by a specific user (no OwnerId).
/// Rules evaluate pregnancy diary entries against configured thresholds.
/// </summary>
public class AlertRule
{
    /// <summary>
    /// Unique identifier (GUID).
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Alert rule name (e.g., "High Temperature", "Abnormal Appetite").
    /// Must be unique.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Species this rule applies to: Dog or Cat.
    /// </summary>
    public string Species { get; private set; } = string.Empty;

    /// <summary>
    /// Severity level: Normal, Attention, Emergency.
    /// Determines UI prominence and user notification urgency.
    /// </summary>
    public string Severity { get; private set; } = AlertSeverity.Normal.Value;

    /// <summary>
    /// Human-readable description of this alert rule.
    /// Explains when and why the alert triggers.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Conditions as JSON or comma-separated criteria.
    /// Example: {"temperature_max": 39.5, "appetite": "decreased"}.
    /// </summary>
    public string Conditions { get; private set; } = string.Empty;

    /// <summary>
    /// Whether this rule is currently active and should be evaluated.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Shadow property: CreatedAt (audit trail, reference data).
    /// </summary>

    /// <summary>
    /// Creates a new AlertRule instance.
    /// </summary>
    /// <param name="id">Alert rule unique identifier.</param>
    /// <param name="name">Alert rule name (unique, required).</param>
    /// <param name="species">Species (Dog or Cat).</param>
    /// <param name="severity">Severity level (Normal, Attention, Emergency).</param>
    /// <param name="description">Rule description.</param>
    /// <param name="conditions">Rule conditions (JSON or comma-separated).</param>
    public AlertRule(
        Guid id,
        string name,
        string species,
        string severity,
        string description,
        string conditions)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Alert rule ID cannot be empty.", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Alert rule name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(species))
            throw new ArgumentException("Species is required.", nameof(species));

        // Validate species via value object
        _ = ValueObjects.Species.FromString(species);

        if (string.IsNullOrWhiteSpace(severity))
            throw new ArgumentException("Severity is required.", nameof(severity));

        // Validate severity via value object
        _ = ValueObjects.AlertSeverity.FromString(severity);

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        if (string.IsNullOrWhiteSpace(conditions))
            throw new ArgumentException("Conditions are required.", nameof(conditions));

        // Basic JSON validation if starts with { (optional)
        if (conditions.TrimStart().StartsWith('{'))
        {
            try
            {
                System.Text.Json.JsonDocument.Parse(conditions);
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw new ArgumentException("Conditions must be valid JSON.", nameof(conditions), ex);
            }
        }

        Id = id;
        Name = name;
        Species = species;
        Severity = severity;
        Description = description;
        Conditions = conditions;
    }

    /// <summary>
    /// Deactivates this alert rule.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Reactivates this alert rule.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }
}
