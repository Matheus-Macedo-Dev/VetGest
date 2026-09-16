using Microsoft.AspNetCore.Identity;

namespace VetGest.Infrastructure.Identity;

/// <summary>
/// Extended IdentityUser for VetGest with role-based authorization.
/// Roles: "Vet" (veterinarian) or "Tutor" (pet owner).
/// Authorization checks are also resource-based to ensure users can only access their own data.
/// </summary>
public class ApplicationUser : IdentityUser<string>
{
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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the user was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Initialize a new ApplicationUser.
    /// </summary>
    public ApplicationUser()
    {
        Id = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Initialize a new ApplicationUser with email.
    /// </summary>
    public ApplicationUser(string email) : this()
    {
        Email = email;
        UserName = email;
    }
}
