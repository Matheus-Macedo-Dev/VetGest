namespace VetGest.Contracts.Identity;

/// <summary>
/// Application user entity extending ASP.NET Core Identity.
/// Used for authentication and role-based authorization.
/// Roles: "Vet" (veterinarian) or "Tutor" (pet owner).
/// Authorization checks are resource-based to ensure users can only access their own data.
/// 
/// Note: This lives in Contracts (shared) to avoid Domain depending on Infrastructure/AspNetCore.
/// </summary>
public class ApplicationUserDto
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// User's email address (unique).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's username.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Full name of the user.
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Whether this user account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the user was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }
}
