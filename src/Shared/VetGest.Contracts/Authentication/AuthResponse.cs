using System.ComponentModel.DataAnnotations;

namespace VetGest.Contracts.Authentication;

/// <summary>
/// Response returned after successful user authentication.
/// Contains JWT token and basic user information.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT bearer token for authenticated requests.
    /// Include as: Authorization: Bearer {token}
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Token type (always "Bearer" for JWT).
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Token expiration time in seconds from now.
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// The authenticated user's unique identifier.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The authenticated user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The authenticated user's full name (if available).
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// User roles (e.g., "Vet", "Tutor").
    /// Authorization checks combine role and resource ownership.
    /// </summary>
    public string[] Roles { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Request to login with email and password.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// User's email address.
    /// </summary>
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password (sent over HTTPS only).
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Request to register a new user account.
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// User's email address (unique identifier).
    /// </summary>
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password (minimum 8 characters with complexity).
    /// </summary>
    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Password confirmation (must match Password).
    /// </summary>
    [Required, Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// User's full name.
    /// </summary>
    [Required, StringLength(256, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Retained for client compatibility, but ignored by the API.
    /// New registrations always receive the Tutor role; Vet accounts are provisioned separately.
    /// </summary>
    public string Role { get; set; } = "Tutor";
}
