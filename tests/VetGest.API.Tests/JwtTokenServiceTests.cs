using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VetGest.API.Authentication;

namespace VetGest.API.Tests;

public sealed class JwtTokenServiceTests
{
    private const string Secret = "test-secret-key-with-at-least-32-characters";

    [Fact]
    public void GenerateToken_preserves_tutor_role_for_jwt_validation()
    {
        var tokenService = new JwtTokenService(Secret, "VetGest.API", "VetGest.Client");
        var token = tokenService.GenerateToken("user-1", "tutor@example.com", ["Tutor"]);

        var principal = new JwtSecurityTokenHandler().ValidateToken(
            token,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "VetGest.API",
                ValidateAudience = true,
                ValidAudience = "VetGest.Client",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                NameClaimType = ClaimTypes.NameIdentifier,
                RoleClaimType = ClaimTypes.Role
            },
            out _);

        Assert.Equal("user-1", principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.True(principal.IsInRole("Tutor"));
        Assert.False(principal.IsInRole("Vet"));
    }
}
