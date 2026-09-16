using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace VetGest.API.Authorization;

/// <summary>
/// Resource ownership requirement for authorization checks.
/// Ensures users can only access resources they own.
/// </summary>
public class ResourceOwnershipRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// The claim type that contains the resource owner ID.
    /// Defaults to NameIdentifier (user ID).
    /// </summary>
    public string OwnerClaimType { get; set; } = ClaimTypes.NameIdentifier;
}

/// <summary>
/// Authorization handler for resource ownership checks.
/// Verifies that the current user matches the resource owner ID.
/// Used alongside role-based authorization for defense in depth.
/// </summary>
public class ResourceOwnershipHandler : AuthorizationHandler<ResourceOwnershipRequirement>
{
    /// <summary>
    /// Verify that the current user is the owner of the requested resource.
    /// </summary>
    /// <remarks>
    /// This handler checks the "owner" claim in the HTTP context items.
    /// The controller must populate context.Items["owner"] with the resource owner ID.
    /// </remarks>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceOwnershipRequirement requirement)
    {
        // This handler is typically used within controller methods that have already
        // loaded the resource. The resource owner ID is passed via context.Items.

        if (context.Resource is not HttpContext httpContext)
        {
            return Task.CompletedTask;
        }

        var userIdClaim = context.User.FindFirst(requirement.OwnerClaimType);
        if (userIdClaim == null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        var userId = userIdClaim.Value;

        // Check if the context has an owner claim set
        if (httpContext.Items.TryGetValue("owner", out var ownerObj) && ownerObj is string owner)
        {
            if (userId == owner)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
            return Task.CompletedTask;
        }

        // If no owner is set in items, fail the authorization
        context.Fail();
        return Task.CompletedTask;
    }
}
