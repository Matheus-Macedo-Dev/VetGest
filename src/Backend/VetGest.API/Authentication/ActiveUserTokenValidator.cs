using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using VetGest.Infrastructure.Identity;

namespace VetGest.API.Authentication;

/// <summary>
/// Rejects tokens of users that were deactivated or deleted, so disabling an account takes effect immediately.
/// </summary>
public static class ActiveUserTokenValidator
{
    public static async Task ValidateAsync(TokenValidatedContext context)
    {
        var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = string.IsNullOrWhiteSpace(userId) ? null : await userManager.FindByIdAsync(userId);

        if (user is null || !user.IsActive)
            context.Fail("A conta está inativa.");
    }
}
