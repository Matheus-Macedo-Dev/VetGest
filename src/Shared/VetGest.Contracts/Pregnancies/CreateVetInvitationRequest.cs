using System.ComponentModel.DataAnnotations;

namespace VetGest.Contracts.Pregnancies;

public sealed class CreateVetInvitationRequest
{
    [Required]
    public Guid PetId { get; set; }

    [Required]
    public Guid PregnancyId { get; set; }

    [Required]
    [StringLength(128, MinimumLength = 1)]
    public string TutorUserId { get; set; } = string.Empty;
}
