namespace VetGest.Contracts.Admin;

/// <summary>
/// Result of creating a veterinarian: the account and its first setup token.
/// </summary>
public class CreatedVetDto
{
    public AdminVetDto Vet { get; set; } = new();
    public VetSetupLinkDto Setup { get; set; } = new();
}
