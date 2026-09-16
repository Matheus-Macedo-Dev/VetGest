namespace VetGest.Contracts.Pregnancies;

public sealed class ExaminationReminderDto
{
    public string Code { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? RecommendedWindow { get; init; }
    public DateOnly? EstimatedDate { get; init; }
    public bool IsPast { get; init; }
    public bool IsDueSoon { get; init; }
    public string Disclaimer { get; init; } = string.Empty;
}