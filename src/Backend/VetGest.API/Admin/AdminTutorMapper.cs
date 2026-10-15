using VetGest.Contracts.Admin;
using VetGest.Infrastructure.Identity;

namespace VetGest.API.Admin;

public static class AdminTutorMapper
{
    public static AdminTutorDto ToDto(ApplicationUser user) => new()
    {
        UserId = user.Id,
        FullName = user.FullName ?? string.Empty,
        Email = user.Email ?? string.Empty,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };
}
