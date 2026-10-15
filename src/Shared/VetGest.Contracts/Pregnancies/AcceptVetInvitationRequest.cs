using System.ComponentModel.DataAnnotations;

namespace VetGest.Contracts.Pregnancies;

public sealed class AcceptVetInvitationRequest
{
    [Required]
    [StringLength(50, MinimumLength = 6)]
    public string InvitationCode { get; set; } = string.Empty;

    /// <summary>Pet whose pregnancy the tutor shares. Required when redeeming a vet-issued code.</summary>
    public Guid? PetId { get; set; }

    /// <summary>Pregnancy the tutor shares. Required when redeeming a vet-issued code.</summary>
    public Guid? PregnancyId { get; set; }
}
