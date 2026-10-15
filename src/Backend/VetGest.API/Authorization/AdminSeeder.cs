using Microsoft.AspNetCore.Identity;
using VetGest.Infrastructure.Identity;

namespace VetGest.API.Authorization;

/// <summary>
/// Creates the first administrator from configuration (Admin:Email / Admin:Password). Never self-registered.
/// </summary>
public static class AdminSeeder
{
    public static async Task EnsureAdminAsync(IServiceProvider services)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var email = configuration["Admin:Email"]?.Trim();
        var password = configuration["Admin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
            return;

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(AdminSeeder));

        if ((await userManager.GetUsersInRoleAsync(RoleSeeder.AdminRole)).Count > 0)
            return;

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser(email) { FullName = "Administrador", EmailConfirmed = true };
            var created = await userManager.CreateAsync(user, password);
            if (!created.Succeeded)
            {
                logger.LogError("Admin bootstrap failed: {Errors}", string.Join("; ", created.Errors.Select(error => error.Code)));
                return;
            }
        }

        var roleResult = await userManager.AddToRoleAsync(user, RoleSeeder.AdminRole);
        if (!roleResult.Succeeded)
            logger.LogError("Admin bootstrap could not assign the Admin role.");
    }
}
