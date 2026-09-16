namespace VetGest.Domain.ValueObjects;

/// <summary>
/// Immutable value object representing a pet species.
/// Supports: Dog, Cat (with species-specific content separation).
/// </summary>
public readonly struct Species : IEquatable<Species>
{
    /// <summary>
    /// Species value. One of: "Dog", "Cat".
    /// </summary>
    public string Value { get; }

    private Species(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Species value cannot be empty.", nameof(value));

        Value = value switch
        {
            nameof(Dog) => nameof(Dog),
            nameof(Cat) => nameof(Cat),
            _ => throw new ArgumentException($"Invalid species: {value}. Must be 'Dog' or 'Cat'.", nameof(value))
        };
    }

    public static readonly Species Dog = new(nameof(Dog));
    public static readonly Species Cat = new(nameof(Cat));

    public static Species FromString(string value)
    {
        return new Species(value);
    }

    public override string ToString() => Value;

    public bool Equals(Species other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is Species other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Species left, Species right) => left.Equals(right);
    public static bool operator !=(Species left, Species right) => !left.Equals(right);
}
