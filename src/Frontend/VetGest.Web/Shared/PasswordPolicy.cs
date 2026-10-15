namespace VetGest.Web.Shared;

/// <summary>
/// Client-side mirror of the API password policy (8+ chars, upper, lower, digit) plus a special character recommendation enforced by the UI.
/// </summary>
public static class PasswordPolicy
{
    public sealed record Rule(string Label, Func<string, bool> IsSatisfied);

    public static IReadOnlyList<Rule> Rules { get; } =
    [
        new("Pelo menos 8 caracteres", password => password.Length >= 8),
        new("Ao menos 1 letra maiúscula", password => password.Any(char.IsUpper)),
        new("Ao menos 1 letra minúscula", password => password.Any(char.IsLower)),
        new("Ao menos 1 número", password => password.Any(char.IsDigit)),
        new("Ao menos 1 caractere especial", password => password.Any(ch => !char.IsLetterOrDigit(ch)))
    ];

    public static bool IsSatisfied(string? password) =>
        !string.IsNullOrEmpty(password) && Rules.All(rule => rule.IsSatisfied(password));

    public static int CountSatisfied(string? password) =>
        string.IsNullOrEmpty(password) ? 0 : Rules.Count(rule => rule.IsSatisfied(password));
}
