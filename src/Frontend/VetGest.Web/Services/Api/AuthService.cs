using System.Net.Http.Json;
using VetGest.Contracts.Authentication;
using VetGest.Contracts.Common;
using VetGest.Web.Services.Auth;

namespace VetGest.Web.Services.Api;

public sealed class AuthService(HttpClient httpClient, CustomAuthenticationStateProvider authStateProvider)
    : ApiClient(httpClient)
{
    public Task<ApiCallResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default) =>
        AuthenticateAsync("login", request, cancellationToken);

    public Task<ApiCallResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default) =>
        AuthenticateAsync("register", request, cancellationToken);

    public Task LogoutAsync() => authStateProvider.SignOutAsync();

    private async Task<ApiCallResult<AuthResponse>> AuthenticateAsync<T>(string action, T request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/auth/{action}")
        {
            Content = JsonContent.Create(request)
        };
        var result = await SendAsync<AuthResponse>(message, cancellationToken);
        if (result.Succeeded && result.Data is not null)
        {
            await authStateProvider.SignInAsync(result.Data.AccessToken);
        }

        return result;
    }
}