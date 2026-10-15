namespace VetGest.Contracts.Authentication;

/// <summary>
/// Shared constants for the password transport envelope exchanged between the browser and the API.
/// The envelope is an extra layer on top of TLS so the password never appears in a request body.
/// </summary>
public static class PasswordTransport
{
    /// <summary>Algorithm identifier reported by the public key endpoint.</summary>
    public const string Algorithm = "RSA-OAEP-256";

    /// <summary>
    /// Validation error key returned when an envelope cannot be decrypted. Clients treat this as a
    /// signal to refetch the public key and retry once, because the API rotates its key on restart.
    /// </summary>
    public const string EncryptedPasswordErrorKey = "EncryptedPassword";

    /// <summary>
    /// Maximum password length. Bounded so the encrypted envelope always fits inside the RSA-3072
    /// OAEP-SHA256 payload limit of 318 bytes, even for multi-byte accented characters.
    /// </summary>
    public const int MaxPasswordLength = 128;

    /// <summary>How far an envelope timestamp may drift from server time before it is rejected.</summary>
    public static readonly TimeSpan MaxEnvelopeAge = TimeSpan.FromMinutes(5);
}
