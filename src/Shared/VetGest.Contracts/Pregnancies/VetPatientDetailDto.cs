namespace VetGest.Contracts.Pregnancies;

/// <summary>
/// Read-only view of what the tutor registered for the shared pregnancy.
/// </summary>
public sealed class VetPatientDetailDto
{
    public VetPatientDto Patient { get; set; } = new();
    public IReadOnlyList<DiaryEntryDto> Diary { get; set; } = Array.Empty<DiaryEntryDto>();

    /// <summary>Reminder that tutor observations are not a diagnosis and the due date is an estimate.</summary>
    public string Notice { get; set; } = string.Empty;
}
