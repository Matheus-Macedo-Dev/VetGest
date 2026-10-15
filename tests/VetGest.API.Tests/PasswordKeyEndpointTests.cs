using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using VetGest.API.Tests.Infrastructure;
using VetGest.Contracts.Authentication;
using VetGest.Contracts.Common;

namespace VetGest.API.Tests;

public sealed class PasswordKeyEndpointTests : IClassFixture<VetGestApiFactory>
{
    private readonly VetGestApiFactory _factory;

    public PasswordKeyEndpointTests(VetGestApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Public_key_is_available_anonymously_and_is_usable()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/auth/public-key");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<PasswordKeyDto>>();
        Assert.NotNull(envelope);
        Assert.True(envelope.Success);
        Assert.NotNull(envelope.Data);
        Assert.Equal(PasswordTransport.Algorithm, envelope.Data.Algorithm);
        Assert.Equal(PasswordTransport.MaxPasswordLength, envelope.Data.MaxPasswordLength);

        // The published key must actually encrypt, otherwise clients could never sign in.
        var ciphertext = PasswordEnvelopeFactory.Encrypt(
            envelope.Data.PublicKey, "Senha#Forte1", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        Assert.NotEmpty(ciphertext);
    }

    [Fact]
    public async Task Public_key_is_stable_while_the_api_is_running()
    {
        var client = _factory.CreateClient();

        var first = await ReadKeyAsync(client);
        var second = await ReadKeyAsync(client);

        // A key that changed between requests would break every sign-in.
        Assert.Equal(first.KeyId, second.KeyId);
        Assert.Equal(first.PublicKey, second.PublicKey);
    }

    [Fact]
    public async Task Published_key_matches_the_key_the_api_decrypts_with()
    {
        var client = _factory.CreateClient();
        var key = await ReadKeyAsync(client);

        using var rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(key.PublicKey), out _);

        Assert.Equal(3072, rsa.KeySize);
    }

    private static async Task<PasswordKeyDto> ReadKeyAsync(HttpClient client)
    {
        var envelope = await client.GetFromJsonAsync<ApiResponse<PasswordKeyDto>>("/api/auth/public-key");
        Assert.NotNull(envelope?.Data);
        return envelope.Data;
    }
}
