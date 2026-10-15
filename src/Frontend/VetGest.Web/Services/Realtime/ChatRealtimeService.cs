using Microsoft.AspNetCore.SignalR.Client;
using VetGest.Contracts.Chat;
using VetGest.Web.Services.Auth;

namespace VetGest.Web.Services.Realtime;

/// <summary>
/// Live chat client. Messages are relayed by the API and are never stored, so history is lost on reload.
/// </summary>
public sealed class ChatRealtimeService : IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private readonly IBrowserTokenStore _tokenStore;
    private HubConnection? _connection;

    public ChatRealtimeService(HttpClient httpClient, IBrowserTokenStore tokenStore)
    {
        _httpClient = httpClient;
        _tokenStore = tokenStore;
    }

    public event Func<ChatMessageDto, Task>? MessageReceived;

    public event Func<Guid, Task>? ChatClosed;

    public event Func<HubConnectionState, Task>? StateChanged;

    public HubConnectionState State => _connection?.State ?? HubConnectionState.Disconnected;

    public async Task EnsureConnectedAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is { State: HubConnectionState.Connected or HubConnectionState.Connecting or HubConnectionState.Reconnecting })
            return;

        if (_connection is not null)
            await _connection.DisposeAsync();

        _connection = new HubConnectionBuilder()
            .WithUrl(new Uri(_httpClient.BaseAddress!, "hubs/chat"), options =>
            {
                options.AccessTokenProvider = () => _tokenStore.GetAsync().AsTask();
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<ChatMessageDto>(ChatHubMethods.MessageReceived, async message =>
        {
            if (MessageReceived is not null)
                await MessageReceived.Invoke(message);
        });

        _connection.On<Guid>(ChatHubMethods.ChatClosed, async connectionId =>
        {
            if (ChatClosed is not null)
                await ChatClosed.Invoke(connectionId);
        });

        _connection.Reconnected += _ => NotifyStateAsync();
        _connection.Reconnecting += _ => NotifyStateAsync();
        _connection.Closed += _ => NotifyStateAsync();

        await _connection.StartAsync(cancellationToken);
        await NotifyStateAsync();
    }

    public async Task JoinAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken);
        await _connection!.InvokeAsync("JoinChat", connectionId, cancellationToken);
    }

    public async Task LeaveAsync(Guid connectionId, CancellationToken cancellationToken = default)
    {
        if (_connection is { State: HubConnectionState.Connected })
            await _connection.InvokeAsync("LeaveChat", connectionId, cancellationToken);
    }

    public async Task SendAsync(Guid connectionId, string text, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken);
        await _connection!.InvokeAsync("SendMessage", connectionId, text, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is null)
            return;

        await _connection.DisposeAsync();
        _connection = null;
    }

    private Task NotifyStateAsync() => StateChanged?.Invoke(State) ?? Task.CompletedTask;
}
