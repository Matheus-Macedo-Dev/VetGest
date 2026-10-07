using System.ComponentModel.DataAnnotations;

namespace VetGest.Contracts.Pregnancies;

public sealed class AcceptVetInvitationRequest
{
    [Required]
    [StringLength(50, MinimumLength = 6)]
    public string InvitationCode { get; set; } = string.Empty;
}
