namespace VetGest.Contracts.Authentication;

/// <summary>
/// Public key used by clients to encrypt a password before sending it to the API.
/// </summary>
public class PasswordKeyDto
{
    /// <summary>
    /// Identifies the current key. Changes whenever the API restarts and regenerates its key pair.
    /// </summary>
    public string KeyId { get; set; } = string.Empty;

    /// <summary>Algorithm the client must use. Always <see cref="PasswordTransport.Algorithm"/>.</summary>
    public string Algorithm { get; set; } = PasswordTransport.Algorithm;

    /// <summary>Base64-encoded SPKI public key.</summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>Maximum password length accepted by the envelope.</summary>
    public int MaxPasswordLength { get; set; } = PasswordTransport.MaxPasswordLength;
}
