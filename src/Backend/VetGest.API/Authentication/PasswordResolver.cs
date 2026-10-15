using Microsoft.Extensions.Options;
using VetGest.Contracts.Authentication;

namespace VetGest.API.Authentication;

/// <summary>
/// Resolves the effective password for an auth request, accepting either an encrypted envelope or
/// a plaintext field when plaintext is explicitly allowed.
/// </summary>
public sealed class PasswordResolver(IPasswordTransportProtector protector, IOptions<AuthTransportOptions> options)
{
    private readonly AuthTransportOptions _options = options.Value;

    /// <summary>
    /// Resolves one password field.
    /// </summary>
    /// <param name="encrypted">Encrypted envelope, preferred when present.</param>
    /// <param name="plaintext">Plaintext fallback, honoured only when plaintext is allowed.</param>
    /// <param name="fieldName">Contract field name used for the validation error.</param>
    public PasswordResolution Resolve(string? encrypted, string? plaintext, string fieldName)
    {
        if (!string.IsNullOrWhiteSpace(encrypted))
        {
            var result = protector.TryUnprotect(encrypted);

            // Signals the client to refetch the public key and retry once, since the API
            // regenerates its key pair on restart.
            return result.Succeeded
                ? PasswordResolution.Success(result.Password)
                : PasswordResolution.Failure(PasswordTransport.EncryptedPasswordErrorKey,
                    "Não foi possível proteger sua senha. Tente novamente.");
        }

        if (!string.IsNullOrEmpty(plaintext))
        {
            return _options.AllowPlaintextPassword
                ? PasswordResolution.Success(plaintext)
                : PasswordResolution.Failure(PasswordTransport.EncryptedPasswordErrorKey,
                    "Este servidor exige que a senha seja criptografada pelo aplicativo.");
        }

        return PasswordResolution.Failure(fieldName, "Informe sua senha.");
    }
}

/// <summary>Either a usable password or the validation error to return.</summary>
public readonly record struct PasswordResolution(bool Succeeded, string Password, string ErrorKey, string ErrorMessage)
{
    public static PasswordResolution Success(string password) => new(true, password, string.Empty, string.Empty);

    public static PasswordResolution Failure(string key, string message) => new(false, string.Empty, key, message);

    public Dictionary<string, string[]> ToErrors() => new() { [ErrorKey] = [ErrorMessage] };
}
