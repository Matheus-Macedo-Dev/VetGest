using System.Text.Json;
using VetGest.Domain.Entities;

namespace VetGest.Application.Alerts;

/// <summary>
/// Evaluates whether a single <see cref="AlertRule"/> is triggered by a diary entry.
/// Handles both the JSON-object and comma-separated-keyword <c>Conditions</c> formats.
/// </summary>
internal static class AlertConditionEvaluator
{
    public static bool IsTriggered(AlertRule rule, PregnancyDiaryEntry entry)
    {
        var conditions = rule.Conditions.TrimStart();
        return conditions.StartsWith('{')
            ? EvaluateJsonConditions(conditions, entry)
            : EvaluateSymptomKeywords(conditions, entry);
    }

    private static bool EvaluateJsonConditions(string conditions, PregnancyDiaryEntry entry)
    {
        using var document = JsonDocument.Parse(conditions);
        var root = document.RootElement;

        if (root.TryGetProperty("appetite", out var appetiteElement))
            return string.Equals(entry.Appetite, appetiteElement.GetString(), StringComparison.OrdinalIgnoreCase);

        var hasMin = root.TryGetProperty("temperature_min", out var minElement);
        var hasMax = root.TryGetProperty("temperature_max", out var maxElement);
        if (!hasMin && !hasMax)
            return false;

        if (entry.Temperature is not decimal temperature)
            return false;

        // Boundary convention: min-only rules trigger on temperature >= min; range rules are inclusive on both ends.
        if (hasMin && hasMax)
            return temperature >= minElement.GetDecimal() && temperature <= maxElement.GetDecimal();

        return hasMin ? temperature >= minElement.GetDecimal() : temperature <= maxElement.GetDecimal();
    }

    private static bool EvaluateSymptomKeywords(string conditions, PregnancyDiaryEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.SymptomsList))
            return false;

        var keywords = conditions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var symptoms = entry.SymptomsList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return keywords.Any(keyword => symptoms.Any(symptom => symptom.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
    }
}
