using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using VetGest.Contracts.Pregnancies;
using VetGest.Web.Services.Auth;

namespace VetGest.Web.Services.Realtime;

public sealed class VetConnectionRealtimeService : IAsyncDisposable
{
    private readonly NavigationManager _navigation;
    private readonly IBrowserTokenStore _tokenStore;
    private HubConnection? _connection;

    public VetConnectionRealtimeService(NavigationManager navigation, IBrowserTokenStore tokenStore)
    {
        _navigation = navigation;
        _tokenStore = tokenStore;
    }

    public event Func<VetConnectionUpdatedEvent, Task>? ConnectionUpdated;

    public async Task EnsureConnectedAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is { State: HubConnectionState.Connected or HubConnectionState.Connecting or HubConnectionState.Reconnecting })
            return;

        if (_connection is not null)
            await _connection.DisposeAsync();

        _connection = new HubConnectionBuilder()
            .WithUrl(_navigation.ToAbsoluteUri("hubs/vet-connections"), options =>
            {
                options.AccessTokenProvider = () => _tokenStore.GetAsync().AsTask();
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<VetConnectionUpdatedEvent>(VetConnectionHubMethods.ConnectionUpdated, async update =>
        {
            if (ConnectionUpdated is null)
                return;

            await ConnectionUpdated.Invoke(update);
        });

        await _connection.StartAsync(cancellationToken);
    }

    public async Task JoinPregnancyAsync(Guid pregnancyId, CancellationToken cancellationToken = default)
    {
        if (pregnancyId == Guid.Empty)
            return;

        await EnsureConnectedAsync(cancellationToken);
        await _connection!.InvokeAsync("JoinPregnancy", pregnancyId, cancellationToken);
    }

    public async Task LeavePregnancyAsync(Guid pregnancyId, CancellationToken cancellationToken = default)
    {
        if (_connection is null || pregnancyId == Guid.Empty)
            return;

        await _connection.InvokeAsync("LeavePregnancy", pregnancyId, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is null)
            return;

        await _connection.DisposeAsync();
        _connection = null;
    }
}
