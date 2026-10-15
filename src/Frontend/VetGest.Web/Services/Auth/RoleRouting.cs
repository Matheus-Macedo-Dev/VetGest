using System.Security.Claims;

namespace VetGest.Web.Services.Auth;

/// <summary>
/// Maps a signed-in user's role to the page they land on.
/// </summary>
public static class RoleRouting
{
    public static string HomeFor(ClaimsPrincipal user)
    {
        if (user.IsInRole("Admin"))
            return "admin/vets";

        if (user.IsInRole("Vet"))
            return "vet-connection";

        return "pets";
    }
}
