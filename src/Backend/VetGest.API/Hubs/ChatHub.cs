using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using VetGest.Application.Pregnancies;
using VetGest.Contracts.Chat;

namespace VetGest.API.Hubs;

/// <summary>
/// Live chat between the two participants of an Active vet connection. Messages are relayed, never stored.
/// Access is re-verified on every call because SignalR group membership is not an authorization boundary.
/// </summary>
[Authorize(Policy = "AuthenticatedUser")]
public sealed class ChatHub : Hub
{
    private static readonly TimeSpan MinSendInterval = TimeSpan.FromMilliseconds(400);
    private static readonly ConcurrentDictionary<string, DateTime> LastSent = new();

    private readonly IVetConnectionService _service;
    private readonly IUserDirectory _userDirectory;

    public ChatHub(IVetConnectionService service, IUserDirectory userDirectory)
    {
        _service = service;
        _userDirectory = userDirectory;
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

        var trimmed = text?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > ChatHubMethods.MaxMessageLength)
            throw new HubException($"A mensagem deve ter de 1 a {ChatHubMethods.MaxMessageLength} caracteres.");

        var now = DateTime.UtcNow;
        var key = $"{userId}:{connectionId}";
        if (LastSent.TryGetValue(key, out var last) && now - last < MinSendInterval)
            throw new HubException("Você está enviando mensagens rápido demais.");
        LastSent[key] = now;

        var names = await _userDirectory.GetDisplayNamesAsync([userId], Context.ConnectionAborted);
        var message = new ChatMessageDto
        {
            ConnectionId = connectionId,
            SenderUserId = userId,
            SenderName = names.GetValueOrDefault(userId) ?? string.Empty,
            Text = trimmed,
            SentAtUtc = now
        };

        await Clients.Group(ChatHubGroups.Connection(connectionId))
            .SendAsync(ChatHubMethods.MessageReceived, message, Context.ConnectionAborted);
    }

    private async Task<string> RequireParticipantAsync(Guid connectionId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)
            || !await _service.CanChatAsync(connectionId, userId, Context.ConnectionAborted))
            throw new HubException("Acesso não autorizado para este chat.");

        return userId;
    }
}
