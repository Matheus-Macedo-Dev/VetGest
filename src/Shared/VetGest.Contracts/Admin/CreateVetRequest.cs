using System.ComponentModel.DataAnnotations;

namespace VetGest.Contracts.Admin;

/// <summary>
/// Request for an administrator to provision a veterinarian account after checking the CRMV on the CFMV registry.
/// </summary>
public class CreateVetRequest
{
    [Required, StringLength(256, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>CRMV registration number (digits).</summary>
    [Required]
    public string CrmvNumber { get; set; } = string.Empty;

    /// <summary>Two-letter Brazilian state (UF) of the regional council.</summary>
    [Required]
    public string CrmvState { get; set; } = string.Empty;
}
