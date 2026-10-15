using VetGest.Contracts.Authentication;

namespace VetGest.API.Authentication;

/// <summary>
/// Outcome of decrypting a client password envelope. Failure carries no detail so the API never
/// reveals whether an envelope was malformed, stale, or encrypted with a retired key.
/// </summary>
public readonly record struct PasswordUnprotectResult(bool Succeeded, string Password)
{
    public static PasswordUnprotectResult Fail() => new(false, string.Empty);

    public static PasswordUnprotectResult Success(string password) => new(true, password);
}

/// <summary>
/// Decrypts passwords that clients encrypt before sending, so the password never appears in a
/// request body. This layers on top of TLS and does not replace it.
/// </summary>
public interface IPasswordTransportProtector
{
    /// <summary>Public key the client uses to encrypt. Regenerated whenever the API restarts.</summary>
    PasswordKeyDto GetPublicKey();

    /// <summary>Decrypts and validates a base64 envelope produced by the client.</summary>
    PasswordUnprotectResult TryUnprotect(string? encryptedPassword);
}
