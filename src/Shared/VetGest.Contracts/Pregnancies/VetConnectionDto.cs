namespace VetGest.Contracts.Pregnancies;

public sealed class VetConnectionDto
{
    public Guid Id { get; set; }
    public Guid PetId { get; set; }
    public Guid PregnancyId { get; set; }
    public string VetUserId { get; set; } = string.Empty;
    public string TutorUserId { get; set; } = string.Empty;
    public string InvitationCode { get; set; } = string.Empty;
    public DateTime InvitationExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? VetName { get; set; }
    public string? TutorName { get; set; }
    public string? PetName { get; set; }
}
