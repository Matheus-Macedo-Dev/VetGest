using Microsoft.AspNetCore.Identity;

namespace VetGest.API.Authorization;

public static class RoleSeeder
{
    public const string AdminRole = "Admin";

    public static async Task EnsureRolesAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<string>>>();

        foreach (var role in new[] { "Tutor", "Vet", AdminRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<string>
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = role,
                    NormalizedName = role.ToUpperInvariant()
                });
        }
    }
}