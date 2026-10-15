using Microsoft.JSInterop;
using VetGest.Contracts.Push;
using VetGest.Web.Services.Api;

namespace VetGest.Web.Services.Realtime;

/// <summary>
/// Opt-in browser push notifications. The server never receives message text, only a generic "new message" push.
/// </summary>
public sealed class PushClientService(IJSRuntime js, PushApiService api)
{
    private sealed record BrowserSubscription(string Endpoint, string P256dh, string Auth);

    public async Task<PushStatus> GetStatusAsync()
    {
        try
        {
            if (!await js.InvokeAsync<bool>("vgPush.isSupported"))
                return PushStatus.Unsupported;

            var key = await api.GetPublicKeyAsync();
            if (!key.Succeeded || key.Data is not { Enabled: true })
                return PushStatus.ServerDisabled;

            if (await js.InvokeAsync<string>("vgPush.permission") == "denied")
                return PushStatus.Denied;

            return await js.InvokeAsync<string?>("vgPush.currentEndpoint") is null ? PushStatus.Off : PushStatus.On;
        }
        catch (JSException)
        {
            return PushStatus.Unsupported;
        }
    }

    public async Task<PushStatus> EnableAsync()
    {
        var key = await api.GetPublicKeyAsync();
        if (!key.Succeeded || key.Data is not { Enabled: true })
            return PushStatus.ServerDisabled;

        var subscription = await js.InvokeAsync<BrowserSubscription?>("vgPush.subscribe", key.Data.PublicKey);
        if (subscription is null)
            return await GetStatusAsync();

        var saved = await api.SubscribeAsync(new PushSubscribeRequest
        {
            Endpoint = subscription.Endpoint,
            P256dh = subscription.P256dh,
            Auth = subscription.Auth
        });

        return saved.Succeeded ? PushStatus.On : PushStatus.Off;
    }

    public async Task DisableAsync()
    {
        try
        {
            var endpoint = await js.InvokeAsync<string?>("vgPush.unsubscribe");
            if (endpoint is not null)
                await api.UnsubscribeAsync(endpoint);
        }
        catch (JSException)
        {
            // The browser subscription is already gone; nothing to clean up.
        }
    }
}