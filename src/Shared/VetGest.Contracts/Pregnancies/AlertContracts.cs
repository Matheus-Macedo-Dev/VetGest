namespace VetGest.Contracts.Pregnancies;

public enum AlertSeverityLevel
{
    Normal,
    Attention,
    Emergency
}

public sealed class TriggeredAlertDto
{
    public string Name { get; init; } = string.Empty;
    public AlertSeverityLevel Severity { get; init; }
    public string Description { get; init; } = string.Empty;
}

public sealed class TodayAlertsDto
{
    public AlertSeverityLevel HighestSeverity { get; init; }
    public IReadOnlyList<TriggeredAlertDto> TriggeredAlerts { get; init; } = Array.Empty<TriggeredAlertDto>();
    public DateTime? EvaluatedEntryDate { get; init; }
    public string Disclaimer { get; init; } = string.Empty;
}
