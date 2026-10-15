namespace VetGest.Domain.ValueObjects;

/// <summary>
/// Immutable value object for a CRMV registration (regional veterinary council number plus state).
/// Only the format is validated; the registry itself is checked manually against the CFMV search.
/// </summary>
public sealed record Crmv
{
    private static readonly HashSet<string> ValidStates = new(StringComparer.Ordinal)
    {
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE",
        "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    };

    private const int MaxDigits = 6;

    public string Number { get; }

    public string State { get; }

    private Crmv(string number, string state)
    {
        Number = number;
        State = state;
    }

    public static bool TryCreate(string? number, string? state, out Crmv? crmv, out string? error)
    {
        crmv = null;
        var normalizedState = state?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(normalizedState) || !ValidStates.Contains(normalizedState))
        {
            error = "Informe a UF do CRMV (ex.: SP).";
            return false;
        }

        // Registrations are sometimes written with a letter suffix (e.g. "12345-V"); only the numeric part identifies the registration.
        var digits = new string((number ?? string.Empty).Where(char.IsAsciiDigit).ToArray()).TrimStart('0');
        if (digits.Length is 0 or > MaxDigits)
        {
            error = $"O número do CRMV deve ter de 1 a {MaxDigits} dígitos.";
            return false;
        }

        crmv = new Crmv(digits, normalizedState);
        error = null;
        return true;
    }

    public override string ToString() => $"CRMV-{State} {Number}";
}
