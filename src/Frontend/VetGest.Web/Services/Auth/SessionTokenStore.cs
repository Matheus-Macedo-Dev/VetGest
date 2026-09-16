using Microsoft.JSInterop;

namespace VetGest.Web.Services.Auth;

public sealed class SessionTokenStore(IJSRuntime jsRuntime) : IBrowserTokenStore
{
    private const string StorageKey = "vetgest.auth.token";

    public ValueTask<string?> GetAsync() =>
        jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", StorageKey);

    public ValueTask SetAsync(string token) =>
        jsRuntime.InvokeVoidAsync("sessionStorage.setItem", StorageKey, token);

    public ValueTask ClearAsync() =>
        jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", StorageKey);
}