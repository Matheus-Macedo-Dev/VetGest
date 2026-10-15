using System.Net.Http.Json;
using Microsoft.JSInterop;
using VetGest.Contracts.Authentication;
using VetGest.Contracts.Common;

namespace VetGest.Web.Services.Auth;

/// <summary>
/// Raised when a password cannot be encrypted for transport. Carries a message safe to show the user.
/// </summary>
public sealed class PasswordEncryptionException(string message) : Exception(message);

/// <summary>
/// Encrypts passwords with the API's public key so they never appear in a request body.
/// The key is cached for the session and refetched when the API reports that it no longer matches.
/// </summary>
public sealed class PasswordTransportProtector(HttpClient httpClient, IJSRuntime jsRuntime) : IAsyncDisposable
{
    private IJSObjectReference? _module;
    private PasswordKeyDto? _key;

    public async Task<string> EncryptAsync(string? password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(password))
            throw new PasswordEncryptionException("Informe sua senha.");

        if (password.Length > PasswordTransport.MaxPasswordLength)
            throw new PasswordEncryptionException(
                $"A senha deve ter no máximo {PasswordTransport.MaxPasswordLength} caracteres.");

        var module = await LoadModuleAsync(cancellationToken);
        var key = await GetKeyAsync(cancellationToken);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        try
        {
            return await module.InvokeAsync<string>(
                "encryptPassword", cancellationToken, key.PublicKey, password, timestamp);
        }
        catch (JSException exception)
        {
            throw new PasswordEncryptionException(TranslateJsError(exception));
        }
    }

    /// <summary>
    /// Discards the cached key so the next encryption fetches a fresh one. Used after the API
    /// reports a key mismatch, which happens when it restarted and regenerated its key pair.
    /// </summary>
    public void Invalidate() => _key = null;

    private async Task<IJSObjectReference> LoadModuleAsync(CancellationToken cancellationToken)
    {
        try
        {
            return _module ??= await jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", cancellationToken, "./js/password-crypto.js");
        }
        catch (JSException)
        {
            throw new PasswordEncryptionException(
                "Não foi possível carregar a proteção de senha. Recarregue a página e tente novamente.");
        }
    }

    private async Task<PasswordKeyDto> GetKeyAsync(CancellationToken cancellationToken)
    {
        if (_key is not null)
            return _key;

        ApiResponse<PasswordKeyDto>? envelope;
        try
        {
            envelope = await httpClient.GetFromJsonAsync<ApiResponse<PasswordKeyDto>>(
                "api/auth/public-key", cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            throw new PasswordEncryptionException(
                "Não foi possível conectar ao VetGest. Verifique sua conexão e tente novamente.");
        }

        if (envelope?.Success != true || envelope.Data is null || string.IsNullOrWhiteSpace(envelope.Data.PublicKey))
            throw new PasswordEncryptionException("Não foi possível proteger sua senha. Tente novamente.");

        return _key = envelope.Data;
    }

    private static string TranslateJsError(JSException exception) => exception.Message switch
    {
        var message when message.Contains("INSECURE_CONTEXT", StringComparison.Ordinal) =>
            "Este endereço não é seguro (HTTPS). Acesse o VetGest por HTTPS para entrar.",
        var message when message.Contains("PASSWORD_TOO_LONG", StringComparison.Ordinal) =>
            $"A senha deve ter no máximo {PasswordTransport.MaxPasswordLength} caracteres.",
        _ => "Não foi possível proteger sua senha. Tente novamente."
    };

    public async ValueTask DisposeAsync()
    {
        if (_module is null)
            return;

        try
        {
            await _module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // The browser is already gone; nothing to release.
        }
    }
}
