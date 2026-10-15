using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using VetGest.Contracts.Authentication;

namespace VetGest.API.Authentication;

/// <summary>
/// Generates an ephemeral RSA-3072 key pair at startup and decrypts the RSA-OAEP envelopes that
/// clients send in place of a plaintext password.
/// </summary>
/// <remarks>
/// The key lives only in this process and is regenerated on restart. That is intentional: it avoids
/// introducing another stored secret. It also means the deployment must stay on a single API
/// instance, because an envelope encrypted with one instance's key cannot be decrypted by another.
/// </remarks>
public sealed class RsaPasswordTransportProtector : IPasswordTransportProtector
{
    private const int KeySizeBits = 3072;

    private static readonly JsonSerializerOptions EnvelopeOptions = new(JsonSerializerDefaults.Web);

    private readonly byte[] _privateKey;
    private readonly PasswordKeyDto _publicKey;
    private readonly TimeProvider _timeProvider;

    public RsaPasswordTransportProtector(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;

        using var rsa = RSA.Create(KeySizeBits);
        _privateKey = rsa.ExportPkcs8PrivateKey();
        _publicKey = new PasswordKeyDto
        {
            KeyId = Guid.NewGuid().ToString("N"),
            Algorithm = PasswordTransport.Algorithm,
            PublicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo()),
            MaxPasswordLength = PasswordTransport.MaxPasswordLength
        };
    }

    public PasswordKeyDto GetPublicKey() => _publicKey;

    public PasswordUnprotectResult TryUnprotect(string? encryptedPassword)
    {
        if (string.IsNullOrWhiteSpace(encryptedPassword))
            return PasswordUnprotectResult.Fail();

        byte[] ciphertext;
        try
        {
            ciphertext = Convert.FromBase64String(encryptedPassword);
        }
        catch (FormatException)
        {
            return PasswordUnprotectResult.Fail();
        }

        byte[] plaintext;
        try
        {
            // A fresh RSA instance per call: concurrent sign-ins must not share one key handle.
            using var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(_privateKey, out _);
            plaintext = rsa.Decrypt(ciphertext, RSAEncryptionPadding.OaepSHA256);
        }
        catch (CryptographicException)
        {
            return PasswordUnprotectResult.Fail();
        }

        try
        {
            return ReadEnvelope(plaintext);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintext);
        }
    }

    private PasswordUnprotectResult ReadEnvelope(byte[] plaintext)
    {
        PasswordEnvelope? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<PasswordEnvelope>(plaintext, EnvelopeOptions);
        }
        catch (JsonException)
        {
            return PasswordUnprotectResult.Fail();
        }

        if (envelope is null || string.IsNullOrEmpty(envelope.Password))
            return PasswordUnprotectResult.Fail();

        if (envelope.Password.Length > PasswordTransport.MaxPasswordLength)
            return PasswordUnprotectResult.Fail();

        var now = _timeProvider.GetUtcNow();
        var maxAgeMs = (long)PasswordTransport.MaxEnvelopeAge.TotalMilliseconds;
        var nowMs = now.ToUnixTimeMilliseconds();

        // Both values are positive, so the subtraction below cannot overflow.
        if (envelope.TimestampMs <= 0)
            return PasswordUnprotectResult.Fail();

        if (Math.Abs(nowMs - envelope.TimestampMs) > maxAgeMs)
            return PasswordUnprotectResult.Fail();

        return PasswordUnprotectResult.Success(envelope.Password);
    }

    private sealed class PasswordEnvelope
    {
        [JsonPropertyName("p")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("t")]
        public long TimestampMs { get; set; }
    }
}
