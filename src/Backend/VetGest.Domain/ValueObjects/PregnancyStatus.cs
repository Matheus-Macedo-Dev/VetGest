namespace VetGest.Domain.ValueObjects;

/// <summary>
/// Immutable value object representing a pregnancy status.
/// Statuses: Pending, Active, Completed, Cancelled.
/// </summary>
public readonly struct PregnancyStatus : IEquatable<PregnancyStatus>
{
    /// <summary>
    /// Status value. One of: "Pending", "Active", "Completed", "Cancelled".
    /// </summary>
    public string Value { get; }

    private PregnancyStatus(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Status value cannot be empty.", nameof(value));

        Value = value switch
        {
            nameof(Pending) => nameof(Pending),
            nameof(Active) => nameof(Active),
            nameof(Completed) => nameof(Completed),
            nameof(Cancelled) => nameof(Cancelled),
            _ => throw new ArgumentException($"Invalid status: {value}.", nameof(value))
        };
    }

    public static readonly PregnancyStatus Pending = new(nameof(Pending));
    public static readonly PregnancyStatus Active = new(nameof(Active));
    public static readonly PregnancyStatus Completed = new(nameof(Completed));
    public static readonly PregnancyStatus Cancelled = new(nameof(Cancelled));

    public static PregnancyStatus FromString(string value)
    {
        return new PregnancyStatus(value);
    }

    public override string ToString() => Value;

    public bool Equals(PregnancyStatus other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is PregnancyStatus other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(PregnancyStatus left, PregnancyStatus right) => left.Equals(right);
    public static bool operator !=(PregnancyStatus left, PregnancyStatus right) => !left.Equals(right);
}
