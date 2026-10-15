using System.Net.Http.Json;
using VetGest.Contracts.Authentication;
using VetGest.Web.Services.Auth;

namespace VetGest.Web.Services.Api;

public sealed class AuthService(HttpClient httpClient, CustomAuthenticationStateProvider authStateProvider,
    IApiWakeUpService wakeUpService, PasswordTransportProtector passwordProtector)
    : ApiClient(httpClient, wakeUpService)
{
    public Task<ApiCallResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default) =>
        AuthenticateAsync("login", async () => new LoginRequest
        {
            Email = request.Email,
            EncryptedPassword = await passwordProtector.EncryptAsync(request.Password, cancellationToken)
        }, cancellationToken);

    public Task<ApiCallResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default) =>
        AuthenticateAsync("register", async () => new RegisterRequest
        {
            Email = request.Email,
            FullName = request.FullName,
            Role = request.Role,
            EncryptedPassword = await passwordProtector.EncryptAsync(request.Password, cancellationToken),
            EncryptedConfirmPassword = await passwordProtector.EncryptAsync(request.ConfirmPassword, cancellationToken)
        }, cancellationToken);

    public Task<ApiCallResult<AuthResponse>> SetPasswordAsync(SetPasswordRequest request, CancellationToken cancellationToken = default) =>
        AuthenticateAsync("set-password", async () => new SetPasswordRequest
        {
            Email = request.Email,
            Token = request.Token,
            EncryptedNewPassword = await passwordProtector.EncryptAsync(request.NewPassword, cancellationToken),
            EncryptedConfirmPassword = await passwordProtector.EncryptAsync(request.ConfirmPassword, cancellationToken)
        }, cancellationToken);

    public Task LogoutAsync() => authStateProvider.SignOutAsync();

    /// <summary>
    /// Builds the request with the password encrypted, sends it, and retries once if the API reports
    /// that it could not decrypt the envelope. That happens when the API restarted and regenerated
    /// its key pair; the retry is safe because decryption fails before any account work begins.
    /// </summary>
    private async Task<ApiCallResult<AuthResponse>> AuthenticateAsync<TRequest>(
        string action, Func<Task<TRequest>> buildRequest, CancellationToken cancellationToken)
    {
        ApiCallResult<AuthResponse> result;
        try
        {
            result = await PostAsync(action, await buildRequest(), cancellationToken);

            if (IsStaleKey(result))
            {
                passwordProtector.Invalidate();
                result = await PostAsync(action, await buildRequest(), cancellationToken);
            }
        }
        catch (PasswordEncryptionException exception)
        {
            return ApiCallResult<AuthResponse>.Failure(exception.Message);
        }

        if (result.Succeeded && result.Data is not null)
        {
            await authStateProvider.SignInAsync(result.Data.AccessToken);
        }

        return result;
    }

    private async Task<ApiCallResult<AuthResponse>> PostAsync<TRequest>(
        string action, TRequest payload, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/auth/{action}")
        {
            Content = JsonContent.Create(payload)
        };

        return await SendAsync<AuthResponse>(message, cancellationToken);
    }

    private static bool IsStaleKey(ApiCallResult<AuthResponse> result) =>
        !result.Succeeded
        && result.ValidationErrors?.ContainsKey(PasswordTransport.EncryptedPasswordErrorKey) == true;
}
