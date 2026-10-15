using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace VetGest.API.Authorization;

/// <summary>
/// Extension methods for configuring authorization policies and handlers.
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Register authorization policies and handlers.
    /// </summary>
    public static IServiceCollection AddVetGestAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Policy for authenticated users
            options.AddPolicy("AuthenticatedUser", policy =>
                policy.RequireAuthenticatedUser());

            // Policy for veterinarians only
            options.AddPolicy("VetOnly", policy =>
                policy.RequireRole("Vet"));

            // Policy for pet owners (tutors) only
            options.AddPolicy("TutorOnly", policy =>
                policy.RequireRole("Tutor"));

            // Policy for administrators only
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole(RoleSeeder.AdminRole));

            // Policy for resource ownership verification
            options.AddPolicy("ResourceOwnership", policy =>
                policy.AddRequirements(new ResourceOwnershipRequirement()));
        });

        // Register custom authorization handlers
        services.AddScoped<IAuthorizationHandler, ResourceOwnershipHandler>();

        return services;
    }
}
