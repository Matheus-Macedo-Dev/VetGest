using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetGest.Application.Pregnancies;
using VetGest.Infrastructure.Identity;

namespace VetGest.Infrastructure.Persistence;

public sealed class IdentityUserDirectory(UserManager<ApplicationUser> userManager) : IUserDirectory
{
    public async Task<IReadOnlyDictionary<string, string>> GetDisplayNamesAsync(
        IReadOnlyCollection<string> userIds,
        CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
            return new Dictionary<string, string>();

        var users = await userManager.Users
            .Where(user => userIds.Contains(user.Id))
            .Select(user => new { user.Id, user.FullName, user.Email })
            .ToListAsync(cancellationToken);

        return users.ToDictionary(user => user.Id, user => user.FullName ?? user.Email ?? string.Empty);
    }
}
