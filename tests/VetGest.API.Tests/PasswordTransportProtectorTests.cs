using System.Security.Cryptography;
using VetGest.API.Authentication;
using VetGest.API.Tests.Infrastructure;
using VetGest.Contracts.Authentication;

namespace VetGest.API.Tests;

public sealed class PasswordTransportProtectorTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 15, 12, 0, 0, TimeSpan.Zero);

    private readonly FixedTimeProvider _clock = new(Now);
    private readonly RsaPasswordTransportProtector _protector;
    private readonly string _publicKey;

    public PasswordTransportProtectorTests()
    {
        _protector = new RsaPasswordTransportProtector(_clock);
        _publicKey = _protector.GetPublicKey().PublicKey;
    }

    [Theory]
    [InlineData("Senha#Forte1")]
    [InlineData("a")]
    public void Round_trips_a_password(string password)
    {
        var envelope = PasswordEnvelopeFactory.Encrypt(_publicKey, password, Now.ToUnixTimeMilliseconds());

        var result = _protector.TryUnprotect(envelope);

        Assert.True(result.Succeeded);
        Assert.Equal(password, result.Password);
    }

    [Fact]
    public void Round_trips_a_max_length_password_of_accented_characters()
    {
        // Worst case for the RSA payload limit: every character is multi-byte in UTF-8.
        var password = new string('ç', PasswordTransport.MaxPasswordLength);

        var envelope = PasswordEnvelopeFactory.Encrypt(_publicKey, password, Now.ToUnixTimeMilliseconds());
        var result = _protector.TryUnprotect(envelope);

        Assert.True(result.Succeeded);
        Assert.Equal(password, result.Password);
    }

    [Fact]
    public void Public_key_is_a_usable_rsa_3072_spki_key()
    {
        var key = _protector.GetPublicKey();

        using var rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(key.PublicKey), out _);

        Assert.Equal(3072, rsa.KeySize);
        Assert.Equal(PasswordTransport.Algorithm, key.Algorithm);
        Assert.NotEmpty(key.KeyId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not base64 at all!!")]
    [InlineData("YWJjZGVm")]
    public void Rejects_input_that_is_not_a_valid_envelope(string? encrypted)
    {
        Assert.False(_protector.TryUnprotect(encrypted).Succeeded);
    }

    [Fact]
    public void Rejects_a_tampered_envelope()
    {
        var envelope = PasswordEnvelopeFactory.Encrypt(_publicKey, "Senha#Forte1", Now.ToUnixTimeMilliseconds());
        var bytes = Convert.FromBase64String(envelope);
        bytes[^1] ^= 0xFF;

        Assert.False(_protector.TryUnprotect(Convert.ToBase64String(bytes)).Succeeded);
    }

    [Fact]
    public void Rejects_an_envelope_encrypted_with_another_instance_key()
    {
        // Mirrors the API restarting and regenerating its key pair.
        var otherInstance = new RsaPasswordTransportProtector(_clock);
        var envelope = PasswordEnvelopeFactory.Encrypt(
            otherInstance.GetPublicKey().PublicKey, "Senha#Forte1", Now.ToUnixTimeMilliseconds());

        Assert.False(_protector.TryUnprotect(envelope).Succeeded);
    }

    [Fact]
    public void Rejects_a_stale_envelope()
    {
        var envelope = PasswordEnvelopeFactory.Encrypt(_publicKey, "Senha#Forte1", Now.ToUnixTimeMilliseconds());
        _clock.Now = Now.Add(PasswordTransport.MaxEnvelopeAge).AddSeconds(1);

        Assert.False(_protector.TryUnprotect(envelope).Succeeded);
    }

    [Fact]
    public void Accepts_an_envelope_inside_the_age_window()
    {
        var envelope = PasswordEnvelopeFactory.Encrypt(_publicKey, "Senha#Forte1", Now.ToUnixTimeMilliseconds());
        _clock.Now = Now.Add(PasswordTransport.MaxEnvelopeAge).AddSeconds(-1);

        Assert.True(_protector.TryUnprotect(envelope).Succeeded);
    }

    [Fact]
    public void Rejects_an_envelope_dated_too_far_in_the_future()
    {
        var future = Now.Add(PasswordTransport.MaxEnvelopeAge).AddMinutes(1).ToUnixTimeMilliseconds();
        var envelope = PasswordEnvelopeFactory.Encrypt(_publicKey, "Senha#Forte1", future);

        Assert.False(_protector.TryUnprotect(envelope).Succeeded);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(long.MinValue)]
    public void Rejects_a_non_positive_timestamp(long timestamp)
    {
        var envelope = PasswordEnvelopeFactory.Encrypt(_publicKey, "Senha#Forte1", timestamp);

        Assert.False(_protector.TryUnprotect(envelope).Succeeded);
    }

    [Fact]
    public void Rejects_a_password_longer_than_the_limit()
    {
        var password = new string('a', PasswordTransport.MaxPasswordLength + 1);
        var envelope = PasswordEnvelopeFactory.Encrypt(_publicKey, password, Now.ToUnixTimeMilliseconds());

        Assert.False(_protector.TryUnprotect(envelope).Succeeded);
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("{}")]
    [InlineData("{\"p\":\"\",\"t\":1760000000000}")]
    public void Rejects_a_decryptable_payload_that_is_not_a_valid_envelope(string payload)
    {
        var envelope = PasswordEnvelopeFactory.EncryptRaw(_publicKey, payload);

        Assert.False(_protector.TryUnprotect(envelope).Succeeded);
    }
}
