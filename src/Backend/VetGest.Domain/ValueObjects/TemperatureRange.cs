namespace VetGest.Domain.ValueObjects;

/// <summary>
/// Immutable value object representing a temperature range with unit.
/// Used in alert rules for temperature-based thresholds.
/// </summary>
public readonly struct TemperatureRange : IEquatable<TemperatureRange>
{
    /// <summary>
    /// Minimum temperature (inclusive).
    /// </summary>
    public decimal MinTemperature { get; }

    /// <summary>
    /// Maximum temperature (inclusive).
    /// </summary>
    public decimal MaxTemperature { get; }

    /// <summary>
    /// Temperature unit: "C" (Celsius) or "F" (Fahrenheit).
    /// </summary>
    public string Unit { get; }

    public TemperatureRange(decimal minTemperature, decimal maxTemperature, string unit = "C")
    {
        if (minTemperature > maxTemperature)
            throw new ArgumentException("Minimum temperature cannot exceed maximum temperature.", nameof(minTemperature));

        if (unit != "C" && unit != "F")
            throw new ArgumentException("Unit must be 'C' (Celsius) or 'F' (Fahrenheit).", nameof(unit));

        MinTemperature = minTemperature;
        MaxTemperature = maxTemperature;
        Unit = unit;
    }

    /// <summary>
    /// Checks if a given temperature falls within this range.
    /// </summary>
    public bool IsInRange(decimal temperature)
    {
        return temperature >= MinTemperature && temperature <= MaxTemperature;
    }

    /// <summary>
    /// Checks if a given temperature is below the minimum.
    /// </summary>
    public bool IsBelowRange(decimal temperature)
    {
        return temperature < MinTemperature;
    }

    /// <summary>
    /// Checks if a given temperature is above the maximum.
    /// </summary>
    public bool IsAboveRange(decimal temperature)
    {
        return temperature > MaxTemperature;
    }

    public override string ToString() => $"{MinTemperature}–{MaxTemperature}°{Unit}";

    public bool Equals(TemperatureRange other)
    {
        return MinTemperature == other.MinTemperature
            && MaxTemperature == other.MaxTemperature
            && Unit == other.Unit;
    }

    public override bool Equals(object? obj) => obj is TemperatureRange other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(MinTemperature, MaxTemperature, Unit);

    public static bool operator ==(TemperatureRange left, TemperatureRange right) => left.Equals(right);
    public static bool operator !=(TemperatureRange left, TemperatureRange right) => !left.Equals(right);
}
