namespace VetGest.API.Authentication;

/// <summary>
/// Controls how passwords may be supplied to the auth endpoints.
/// </summary>
public sealed class AuthTransportOptions
{
    public const string SectionName = "Auth";

    /// <summary>
    /// Whether a plaintext password field is still accepted. Enabled in Development so curl and
    /// Swagger remain usable; disabled elsewhere so clients must encrypt.
    /// </summary>
    public bool AllowPlaintextPassword { get; set; }
}
