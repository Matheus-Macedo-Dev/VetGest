using Microsoft.Extensions.Options;
using VetGest.API.Authentication;
using VetGest.API.Tests.Infrastructure;
using VetGest.Contracts.Authentication;

namespace VetGest.API.Tests;

public sealed class PasswordResolverTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 15, 12, 0, 0, TimeSpan.Zero);

    private readonly FixedTimeProvider _clock = new(Now);
    private readonly RsaPasswordTransportProtector _protector;

    public PasswordResolverTests() => _protector = new RsaPasswordTransportProtector(_clock);

    private PasswordResolver CreateResolver(bool allowPlaintext) =>
        new(_protector, Options.Create(new AuthTransportOptions { AllowPlaintextPassword = allowPlaintext }));

    private string Encrypt(string password) =>
        PasswordEnvelopeFactory.Encrypt(_protector.GetPublicKey().PublicKey, password, Now.ToUnixTimeMilliseconds());

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Accepts_an_encrypted_password_in_either_mode(bool allowPlaintext)
    {
        var result = CreateResolver(allowPlaintext).Resolve(Encrypt("Senha#Forte1"), null, "Password");

        Assert.True(result.Succeeded);
        Assert.Equal("Senha#Forte1", result.Password);
    }

    [Fact]
    public void Prefers_the_encrypted_value_over_the_plaintext_one()
    {
        var result = CreateResolver(allowPlaintext: true)
            .Resolve(Encrypt("Correta#1"), "Errada#1", "Password");

        Assert.True(result.Succeeded);
        Assert.Equal("Correta#1", result.Password);
    }

    [Fact]
    public void Accepts_plaintext_only_when_allowed()
    {
        var result = CreateResolver(allowPlaintext: true).Resolve(null, "Senha#Forte1", "Password");

        Assert.True(result.Succeeded);
        Assert.Equal("Senha#Forte1", result.Password);
    }

    [Fact]
    public void Rejects_plaintext_when_not_allowed()
    {
        var result = CreateResolver(allowPlaintext: false).Resolve(null, "Senha#Forte1", "Password");

        Assert.False(result.Succeeded);
        Assert.Equal(PasswordTransport.EncryptedPasswordErrorKey, result.ErrorKey);
    }

    [Fact]
    public void Reports_the_retry_key_when_the_envelope_cannot_be_decrypted()
    {
        // The client uses this key as the signal to refetch the public key and retry once.
        var result = CreateResolver(allowPlaintext: true).Resolve("bm90LWFuLWVudmVsb3Bl", null, "Password");

        Assert.False(result.Succeeded);
        Assert.Equal(PasswordTransport.EncryptedPasswordErrorKey, result.ErrorKey);
    }

    [Fact]
    public void Reports_the_field_when_no_password_is_supplied()
    {
        var result = CreateResolver(allowPlaintext: true).Resolve(null, null, "ConfirmPassword");

        Assert.False(result.Succeeded);
        Assert.Equal("ConfirmPassword", result.ErrorKey);
        Assert.Equal(["Informe sua senha."], result.ToErrors()["ConfirmPassword"]);
    }
}
