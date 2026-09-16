using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace VetGest.Web.Services.Auth;

public sealed class CustomAuthenticationStateProvider(IBrowserTokenStore tokenStore) : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private AuthenticationState _currentState = Anonymous;

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(_currentState);

    public async Task InitializeAsync()
    {
        var token = await tokenStore.GetAsync();
        _currentState = CreateState(token);
        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
    }

    public async Task SignInAsync(string token)
    {
        var state = CreateState(token);
        if (!state.User.Identity?.IsAuthenticated ?? true)
        {
            await SignOutAsync();
            return;
        }

        await tokenStore.SetAsync(token);
        _currentState = state;
        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
    }

    public async Task SignOutAsync()
    {
        await tokenStore.ClearAsync();
        _currentState = Anonymous;
        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
    }

    private static AuthenticationState CreateState(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Anonymous;
        }

        try
        {
            var segments = token.Split('.');
            if (segments.Length != 3)
            {
                return Anonymous;
            }

            var payload = segments[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            using var document = JsonDocument.Parse(Convert.FromBase64String(payload));
            var root = document.RootElement;

            if (!root.TryGetProperty("exp", out var expiry) ||
                !expiry.TryGetInt64(out var expirySeconds) ||
                DateTimeOffset.FromUnixTimeSeconds(expirySeconds) <= DateTimeOffset.UtcNow)
            {
                return Anonymous;
            }

            var claims = root.EnumerateObject()
                .SelectMany(property => property.Name switch
                {
                    "role" or "roles" => ReadStringValues(property.Value)
                        .Select(value => new Claim(ClaimTypes.Role, value)),
                    _ => ReadStringValues(property.Value)
                        .Select(value => new Claim(property.Name, value))
                })
                .ToList();

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
        }
        catch (FormatException)
        {
            return Anonymous;
        }
        catch (JsonException)
        {
            return Anonymous;
        }
        catch (ArgumentOutOfRangeException)
        {
            return Anonymous;
        }
    }

    private static IEnumerable<string> ReadStringValues(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Array)
        {
            return value.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString()!)
                .Where(item => !string.IsNullOrWhiteSpace(item));
        }

        return value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString())
            ? [value.GetString()!]
            : [];
    }
}