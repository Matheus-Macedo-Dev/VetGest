using VetGest.Application.Chat;
using VetGest.Application.Pregnancies;
using VetGest.Application.Push;
using VetGest.Contracts.Chat;

namespace VetGest.API.Tests.Infrastructure;

/// <summary>
/// In-memory chat service that honours the participants registered in <see cref="TestVetConnectionService.AllowedChat"/>.
/// </summary>
public sealed class TestChatService(TestVetConnectionService connections) : IChatService
{
    private readonly List<(ChatMessageDto Message, string Recipient, bool Read)> _messages = [];

    public Task<ChatSendResult> SendAsync(Guid connectionId, string senderUserId, string text, CancellationToken cancellationToken)
    {
        if (!connections.AllowedChat.Contains((connectionId, senderUserId)))
            throw new VetConnectionNotFoundException();

        var trimmed = text?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > ChatHubMethods.MaxMessageLength)
            throw new VetConnectionValidationException(new Dictionary<string, string[]> { ["Text"] = ["Mensagem inválida."] });

        var recipient = connections.AllowedChat.First(entry => entry.ConnectionId == connectionId && entry.UserId != senderUserId).UserId;
        var message = new ChatMessageDto
        {
            Id = Guid.NewGuid(),
            ConnectionId = connectionId,
            SenderUserId = senderUserId,
            SenderName = $"Name {senderUserId}",
            Text = trimmed,
            SentAtUtc = DateTime.UtcNow
        };
        _messages.Add((message, recipient, false));
        return Task.FromResult(new ChatSendResult(message, recipient));
    }

    public Task<IReadOnlyList<ChatMessageDto>> GetHistoryAsync(Guid connectionId, string userId, CancellationToken cancellationToken)
    {
        RequireParticipant(connectionId, userId);
        return Task.FromResult<IReadOnlyList<ChatMessageDto>>(_messages.Where(item => item.Message.ConnectionId == connectionId).Select(item => item.Message).ToList());
    }

    public Task MarkReadAsync(Guid connectionId, string userId, CancellationToken cancellationToken)
    {
        RequireParticipant(connectionId, userId);
        for (var i = 0; i < _messages.Count; i++)
        {
            if (_messages[i].Message.ConnectionId == connectionId && _messages[i].Recipient == userId)
                _messages[i] = _messages[i] with { Read = true };
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ChatUnreadDto>> GetUnreadAsync(string userId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ChatUnreadDto>>(_messages
            .Where(item => item.Recipient == userId && !item.Read)
            .GroupBy(item => item.Message.ConnectionId)
            .Select(group => new ChatUnreadDto { ConnectionId = group.Key, UnreadCount = group.Count() })
            .ToList());

    private void RequireParticipant(Guid connectionId, string userId)
    {
        if (!connections.AllowedChat.Contains((connectionId, userId)))
            throw new VetConnectionNotFoundException();
    }
}

public sealed class TestPushSender : IPushSender
{
    public List<(string UserId, PushMessage Message)> Sent { get; } = [];

    public bool Enabled => true;

    public Task SendAsync(string userId, PushMessage message, CancellationToken cancellationToken)
    {
        Sent.Add((userId, message));
        return Task.CompletedTask;
    }
}