using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace VetGest.API.Authentication;

/// <summary>
/// Generates JWT tokens for authenticated users.
/// Tokens include role claims (Vet, Tutor) and user identity.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generate a JWT token for the given user with their roles.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="roles">The user's assigned roles (e.g., "Vet", "Tutor").</param>
    /// <param name="expiresInMinutes">Token expiration time in minutes.</param>
    /// <returns>JWT token string.</returns>
    string GenerateToken(string userId, string email, IEnumerable<string> roles, int expiresInMinutes = 60);
}

/// <summary>
/// Default implementation of JWT token generation.
/// Uses HS256 algorithm with a configured secret.
/// </summary>
public class JwtTokenService : ITokenService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    /// <summary>
    /// Initialize the JWT token service.
    /// </summary>
    /// <param name="secretKey">The secret key for signing tokens (min 32 characters).</param>
    /// <param name="issuer">The token issuer (e.g., "VetGest.API").</param>
    /// <param name="audience">The token audience (e.g., "VetGest.Client").</param>
    public JwtTokenService(string secretKey, string issuer = "VetGest.API", string audience = "VetGest.Client")
    {
        if (string.IsNullOrEmpty(secretKey))
            throw new ArgumentNullException(nameof(secretKey));

        if (secretKey.Length < 32)
            throw new ArgumentException("Secret key must be at least 32 characters long.", nameof(secretKey));

        _secretKey = secretKey;
        _issuer = issuer;
        _audience = audience;
    }

    /// <summary>
    /// Generate a JWT token with user identity and role claims.
    /// </summary>
    public string GenerateToken(string userId, string email, IEnumerable<string> roles, int expiresInMinutes = 60)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim("email", email)
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
