namespace VetGest.Domain.ValueObjects;

/// <summary>
/// Immutable value object representing a vet connection status.
/// Statuses: Pending, Active, Revoked.
/// </summary>
public readonly struct VetConnectionStatus : IEquatable<VetConnectionStatus>
{
    /// <summary>
    /// Status value. One of: "Pending", "Active", "Revoked".
    /// </summary>
    public string Value { get; }

    private VetConnectionStatus(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Status value cannot be empty.", nameof(value));

        Value = value switch
        {
            nameof(Pending) => nameof(Pending),
            nameof(Active) => nameof(Active),
            nameof(Revoked) => nameof(Revoked),
            _ => throw new ArgumentException($"Invalid vet connection status: {value}.", nameof(value))
        };
    }

    public static readonly VetConnectionStatus Pending = new(nameof(Pending));
    public static readonly VetConnectionStatus Active = new(nameof(Active));
    public static readonly VetConnectionStatus Revoked = new(nameof(Revoked));

    public static VetConnectionStatus FromString(string value)
    {
        return new VetConnectionStatus(value);
    }

    public override string ToString() => Value;

    public bool Equals(VetConnectionStatus other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is VetConnectionStatus other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(VetConnectionStatus left, VetConnectionStatus right) => left.Equals(right);
    public static bool operator !=(VetConnectionStatus left, VetConnectionStatus right) => !left.Equals(right);
}
