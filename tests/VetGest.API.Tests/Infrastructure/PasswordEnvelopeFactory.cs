using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace VetGest.API.Tests.Infrastructure;

/// <summary>
/// Produces password envelopes the same way the browser module does, so the server-side protector
/// is exercised against the real wire format rather than an internal shortcut.
/// </summary>
public static class PasswordEnvelopeFactory
{
    // Matches JSON.stringify, which emits raw UTF-8 instead of escaping non-ASCII as \uXXXX.
    // Escaping would inflate accented passwords past the RSA payload limit that the browser never hits.
    private static readonly JsonSerializerOptions BrowserLike = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string Encrypt(string publicKeyBase64, string password, long timestampMs)
    {
        var json = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["p"] = password,
            ["t"] = timestampMs
        }, BrowserLike);

        using var rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKeyBase64), out _);
        return Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(json), RSAEncryptionPadding.OaepSHA256));
    }

    /// <summary>Encrypts a payload that is not a valid envelope, to prove the shape is validated.</summary>
    public static string EncryptRaw(string publicKeyBase64, string rawPayload)
    {
        using var rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKeyBase64), out _);
        return Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(rawPayload), RSAEncryptionPadding.OaepSHA256));
    }
}

/// <summary>Deterministic clock so envelope expiry can be tested without waiting.</summary>
public sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now;
}
