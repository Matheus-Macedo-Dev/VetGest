namespace VetGest.Domain.ValueObjects;

/// <summary>
/// Immutable value object representing an appetite level observation.
/// Appetite: Normal, Decreased, Increased.
/// </summary>
public readonly struct Appetite : IEquatable<Appetite>
{
    /// <summary>
    /// Appetite level. One of: "Normal", "Decreased", "Increased".
    /// </summary>
    public string Value { get; }

    private Appetite(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Appetite value cannot be empty.", nameof(value));

        Value = value switch
        {
            nameof(Normal) => nameof(Normal),
            nameof(Decreased) => nameof(Decreased),
            nameof(Increased) => nameof(Increased),
            _ => throw new ArgumentException($"Invalid appetite: {value}.", nameof(value))
        };
    }

    public static readonly Appetite Normal = new(nameof(Normal));
    public static readonly Appetite Decreased = new(nameof(Decreased));
    public static readonly Appetite Increased = new(nameof(Increased));

    public static Appetite FromString(string value)
    {
        return new Appetite(value);
    }

    public override string ToString() => Value;

    public bool Equals(Appetite other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is Appetite other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Appetite left, Appetite right) => left.Equals(right);
    public static bool operator !=(Appetite left, Appetite right) => !left.Equals(right);
}
