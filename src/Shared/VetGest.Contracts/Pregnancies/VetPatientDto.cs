namespace VetGest.Contracts.Pregnancies;

/// <summary>
/// A tutor's pet/pregnancy shared with a veterinarian through an active connection.
/// </summary>
public sealed class VetPatientDto
{
    public Guid ConnectionId { get; set; }
    public Guid PetId { get; set; }
    public string PetName { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public Guid PregnancyId { get; set; }
    public string PregnancyStatus { get; set; } = string.Empty;
    public DateTime EstimatedDueDate { get; set; }
    public string TutorName { get; set; } = string.Empty;
    public DateTime? ConnectedAt { get; set; }
}
