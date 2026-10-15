namespace VetGest.Contracts.Admin;

/// <summary>
/// Tutor account summary for administrators.
/// </summary>
public class AdminTutorDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
