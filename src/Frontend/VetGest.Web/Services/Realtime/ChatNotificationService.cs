using Microsoft.AspNetCore.Components;
using MudBlazor;
using VetGest.Contracts.Chat;
using VetGest.Web.Services.Api;

namespace VetGest.Web.Services.Realtime;

/// <summary>
/// App-wide unread counters and in-app "new message" toasts. Started once a Vet or Tutor is signed in.
/// </summary>
public sealed class ChatNotificationService
{
    private readonly ChatRealtimeService _realtime;
    private readonly ChatApiService _api;
    private readonly ISnackbar _snackbar;
    private readonly NavigationManager _navigation;
    private readonly Dictionary<Guid, int> _unread = [];
    private bool _started;

    public ChatNotificationService(ChatRealtimeService realtime, ChatApiService api, ISnackbar snackbar, NavigationManager navigation)
    {
        _realtime = realtime;
        _api = api;
        _snackbar = snackbar;
        _navigation = navigation;
    }

    public event Action? Changed;

    public int TotalUnread => _unread.Values.Sum();

    /// <summary>Conversation currently open on screen; its messages are marked as read immediately.</summary>
    public Guid? ActiveConnectionId { get; set; }

    public int UnreadFor(Guid connectionId) => _unread.GetValueOrDefault(connectionId);

    public async Task StartAsync()
    {
        if (_started)
            return;

        _started = true;
        _realtime.NotificationReceived += OnNotificationAsync;
        await RefreshAsync();

        try
        {
            await _realtime.EnsureConnectedAsync();
        }
        catch
        {
            // Offline or API asleep: counters still load from the API and the next call reconnects.
        }
    }

    public void Stop()
    {
        if (!_started)
            return;

        _started = false;
        _realtime.NotificationReceived -= OnNotificationAsync;
        _unread.Clear();
        Changed?.Invoke();
    }

    public async Task RefreshAsync()
    {
        var result = await _api.GetUnreadAsync();
        if (!result.Succeeded)
            return;

        _unread.Clear();
        foreach (var item in result.Data ?? [])
            _unread[item.ConnectionId] = item.UnreadCount;

        Changed?.Invoke();
    }

    public void MarkConversationRead(Guid connectionId)
    {
        if (_unread.Remove(connectionId))
            Changed?.Invoke();
    }

    private Task OnNotificationAsync(ChatNotificationDto notification)
    {
        if (ActiveConnectionId == notification.ConnectionId)
            return Task.CompletedTask;

        _unread[notification.ConnectionId] = UnreadFor(notification.ConnectionId) + 1;
        Changed?.Invoke();

        var sender = string.IsNullOrWhiteSpace(notification.SenderName) ? "seu contato" : notification.SenderName;
        _snackbar.Add($"Nova mensagem de {sender}", Severity.Info, options =>
        {
            options.Action = "Abrir";
            options.OnClick = _ =>
            {
                _navigation.NavigateTo($"chat?connectionId={notification.ConnectionId:D}");
                return Task.CompletedTask;
            };
        });

        return Task.CompletedTask;
    }
}