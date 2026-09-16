namespace VetGest.Contracts.Pregnancies;

public sealed class CareContentDto
{
    public bool IsAvailable { get; init; }
    public string Species { get; init; } = string.Empty;
    public string? Phase { get; init; }
    public Guid? PhaseId { get; init; }
    public int? StartDayGestation { get; init; }
    public int? EndDayGestation { get; init; }
    public string? FetalDevelopment { get; init; }
    public string? MaternalChanges { get; init; }
    public IReadOnlyList<CareGuidanceItemDto> GuidanceItems { get; init; } = Array.Empty<CareGuidanceItemDto>();
    public string PublicationStatus { get; init; } = "Unpublished";
    public string ReviewStatus { get; init; } = "Unreviewed";
    public string? Source { get; init; }
    public string? Reviewer { get; init; }
    public DateTime? ReviewDate { get; init; }
    public string Disclaimer { get; init; } = string.Empty;
    public string UncertaintyStatement { get; init; } = string.Empty;
}

public sealed class CareGuidanceItemDto
{
    public string Title { get; init; } = string.Empty;
    public string Guidance { get; init; } = string.Empty;
}