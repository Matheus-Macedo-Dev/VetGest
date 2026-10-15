namespace VetGest.Application.Push;

/// <summary>
/// The server posts to subscription endpoints, so only HTTPS endpoints of the known browser push services are accepted (SSRF protection).
/// </summary>
public static class PushEndpointPolicy
{
    private static readonly string[] AllowedHostSuffixes =
    [
        "fcm.googleapis.com",
        "push.services.mozilla.com",
        "push.apple.com",
        "notify.windows.com"
    ];

    public static bool IsAllowed(string? endpoint)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || !uri.IsDefaultPort)
            return false;

        var host = uri.Host.ToLowerInvariant();
        return AllowedHostSuffixes.Any(suffix => host == suffix || host.EndsWith("." + suffix, StringComparison.Ordinal));
    }
}