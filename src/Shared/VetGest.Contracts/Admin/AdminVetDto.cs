namespace VetGest.Contracts.Admin;

/// <summary>
/// Veterinarian account summary for administrators.
/// </summary>
public class AdminVetDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Crmv { get; set; } = string.Empty;
    public string CrmvNumber { get; set; } = string.Empty;
    public string CrmvState { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool PasswordSet { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CrmvCheckedAt { get; set; }
}
