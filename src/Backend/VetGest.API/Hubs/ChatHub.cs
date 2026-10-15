using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using VetGest.Application.Chat;
using VetGest.Application.Pregnancies;
using VetGest.Application.Push;
using VetGest.Contracts.Chat;

namespace VetGest.API.Hubs;

/// <summary>
/// Live chat between the two participants of an Active vet connection. Messages are stored for a short window,
/// the recipient is notified (in-app, or by push when offline), and access is re-verified on every call because
/// SignalR group membership is not an authorization boundary.
/// </summary>
[Authorize(Policy = "AuthenticatedUser")]
public sealed class ChatHub : Hub
{
    private static readonly TimeSpan MinSendInterval = TimeSpan.FromMilliseconds(400);
    private static readonly ConcurrentDictionary<string, DateTime> LastSent = new();

    private readonly IVetConnectionService _connections;
    private readonly IChatService _chat;
    private readonly IChatPresenceTracker _presence;
    private readonly IPushSender _pushSender;

    public ChatHub(IVetConnectionService connections, IChatService chat, IChatPresenceTracker presence, IPushSender pushSender)
    {
        _connections = connections;
        _chat = chat;
        _presence = presence;
        _pushSender = pushSender;
    }

    public override Task OnConnectedAsync()
    {
        if (CurrentUserId() is { } userId)
            _presence.Connected(userId);

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (CurrentUserId() is { } userId)
            _presence.Disconnected(userId);

        return base.OnDisconnectedAsync(exception);
    }

    public async Task JoinChat(Guid connectionId)
    {
        await RequireParticipantAsync(connectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, ChatHubGroups.Connection(connectionId));
    }

    public Task LeaveChat(Guid connectionId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, ChatHubGroups.Connection(connectionId));

    public async Task SendMessage(Guid connectionId, string text)
    {
        var userId = await RequireParticipantAsync(connectionId);

        var now = DateTime.UtcNow;
        var key = $"{userId}:{connectionId}";
        if (LastSent.TryGetValue(key, out var last) && now - last < MinSendInterval)
            throw new HubException("Você está enviando mensagens rápido demais.");

        ChatSendResult result;
        try
        {
            result = await _chat.SendAsync(connectionId, userId, text, Context.ConnectionAborted);
        }
        catch (VetConnectionValidationException exception)
        {
            throw new HubException(exception.Errors.Values.SelectMany(messages => messages).FirstOrDefault() ?? "Mensagem inválida.");
        }
        catch (VetConnectionNotFoundException)
        {
            throw new HubException("Acesso não autorizado para este chat.");
        }

        LastSent[key] = now;

        await Clients.Group(ChatHubGroups.Connection(connectionId))
            .SendAsync(ChatHubMethods.MessageReceived, result.Message, Context.ConnectionAborted);

        var notification = new ChatNotificationDto
        {
            ConnectionId = connectionId,
            SenderName = result.Message.SenderName,
            SentAtUtc = result.Message.SentAtUtc
        };
        await Clients.User(result.RecipientUserId)
            .SendAsync(ChatHubMethods.NotificationReceived, notification, Context.ConnectionAborted);

        if (!_presence.IsOnline(result.RecipientUserId))
        {
            var sender = string.IsNullOrWhiteSpace(result.Message.SenderName) ? "alguém" : result.Message.SenderName;
            await _pushSender.SendAsync(
                result.RecipientUserId,
                new PushMessage("VetGest", $"Nova mensagem de {sender}", $"/chat?connectionId={connectionId:D}"),
                Context.ConnectionAborted);
        }
    }

    private string? CurrentUserId() => Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    private async Task<string> RequireParticipantAsync(Guid connectionId)
    {
        var userId = CurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)
            || !await _connections.CanChatAsync(connectionId, userId, Context.ConnectionAborted))
            throw new HubException("Acesso não autorizado para este chat.");

        return userId;
    }
}