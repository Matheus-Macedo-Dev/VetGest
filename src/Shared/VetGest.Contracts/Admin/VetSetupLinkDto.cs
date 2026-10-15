namespace VetGest.Contracts.Admin;

/// <summary>
/// One-time setup token that lets a provisioned veterinarian choose a password.
/// </summary>
public class VetSetupLinkDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
