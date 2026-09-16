namespace VetGest.Domain.ValueObjects;

/// <summary>
/// Immutable value object representing an alert rule severity level.
/// Severity: Normal, Attention, Emergency.
/// </summary>
public readonly struct AlertSeverity : IEquatable<AlertSeverity>
{
    /// <summary>
    /// Severity level. One of: "Normal", "Attention", "Emergency".
    /// </summary>
    public string Value { get; }

    private AlertSeverity(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Severity value cannot be empty.", nameof(value));

        Value = value switch
        {
            nameof(Normal) => nameof(Normal),
            nameof(Attention) => nameof(Attention),
            nameof(Emergency) => nameof(Emergency),
            _ => throw new ArgumentException($"Invalid severity: {value}.", nameof(value))
        };
    }

    public static readonly AlertSeverity Normal = new(nameof(Normal));
    public static readonly AlertSeverity Attention = new(nameof(Attention));
    public static readonly AlertSeverity Emergency = new(nameof(Emergency));

    public static AlertSeverity FromString(string value)
    {
        return new AlertSeverity(value);
    }

    public override string ToString() => Value;

    public bool Equals(AlertSeverity other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is AlertSeverity other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(AlertSeverity left, AlertSeverity right) => left.Equals(right);
    public static bool operator !=(AlertSeverity left, AlertSeverity right) => !left.Equals(right);
}
