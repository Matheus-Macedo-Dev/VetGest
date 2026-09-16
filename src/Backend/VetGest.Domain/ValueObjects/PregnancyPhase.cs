namespace VetGest.Domain.ValueObjects;

/// <summary>
/// Immutable value object representing a pregnancy phase.
/// Phases: Initial, Intermediate, Growth, Final, Birth.
/// </summary>
public readonly struct PregnancyPhase : IEquatable<PregnancyPhase>
{
    /// <summary>
    /// Phase name. One of: "Initial", "Intermediate", "Growth", "Final", "Birth".
    /// </summary>
    public string Value { get; }

    private PregnancyPhase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phase value cannot be empty.", nameof(value));

        Value = value switch
        {
            nameof(Initial) => nameof(Initial),
            nameof(Intermediate) => nameof(Intermediate),
            nameof(Growth) => nameof(Growth),
            nameof(Final) => nameof(Final),
            nameof(Birth) => nameof(Birth),
            _ => throw new ArgumentException($"Invalid phase: {value}.", nameof(value))
        };
    }

    public static readonly PregnancyPhase Initial = new(nameof(Initial));
    public static readonly PregnancyPhase Intermediate = new(nameof(Intermediate));
    public static readonly PregnancyPhase Growth = new(nameof(Growth));
    public static readonly PregnancyPhase Final = new(nameof(Final));
    public static readonly PregnancyPhase Birth = new(nameof(Birth));

    public static PregnancyPhase FromString(string value)
    {
        return new PregnancyPhase(value);
    }

    public override string ToString() => Value;

    public bool Equals(PregnancyPhase other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is PregnancyPhase other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(PregnancyPhase left, PregnancyPhase right) => left.Equals(right);
    public static bool operator !=(PregnancyPhase left, PregnancyPhase right) => !left.Equals(right);
}
