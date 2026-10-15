using VetGest.Application.Pregnancies;
using VetGest.Contracts.Chat;
using VetGest.Domain.Entities;
using VetGest.Domain.ValueObjects;

namespace VetGest.Application.Chat;

public sealed class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly IVetConnectionRepository _connectionRepository;
    private readonly IUserDirectory _userDirectory;
    private readonly ChatServiceOptions _options;

    public ChatService(
        IChatRepository chatRepository,
        IVetConnectionRepository connectionRepository,
        IUserDirectory userDirectory,
        ChatServiceOptions? options = null)
    {
        _chatRepository = chatRepository;
        _connectionRepository = connectionRepository;
        _userDirectory = userDirectory;
        _options = options ?? new ChatServiceOptions();
    }

    public async Task<ChatSendResult> SendAsync(Guid connectionId, string senderUserId, string text, CancellationToken cancellationToken)
    {
        var connection = await RequireParticipantAsync(connectionId, senderUserId, cancellationToken);

        var trimmed = text?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > ChatMessage.MaxTextLength)
        {
            throw new VetConnectionValidationException(new Dictionary<string, string[]>
            {
                ["Text"] = [$"A mensagem deve ter de 1 a {ChatMessage.MaxTextLength} caracteres."]
            });
        }

        var message = new ChatMessage(Guid.NewGuid(), connectionId, senderUserId.Trim(), trimmed, DateTime.UtcNow);
        await _chatRepository.AddAsync(message, cancellationToken);

        var recipient = StringComparer.Ordinal.Equals(connection.VetUserId, message.SenderUserId)
            ? connection.TutorUserId!
            : connection.VetUserId;

        var names = await _userDirectory.GetDisplayNamesAsync([message.SenderUserId], cancellationToken);
        return new ChatSendResult(Map(message, names), recipient);
    }

    public async Task<IReadOnlyList<ChatMessageDto>> GetHistoryAsync(Guid connectionId, string userId, CancellationToken cancellationToken)
    {
        await RequireParticipantAsync(connectionId, userId, cancellationToken);

        var messages = await _chatRepository.ListAsync(connectionId, Since(), cancellationToken);
        var names = await _userDirectory.GetDisplayNamesAsync(
            messages.Select(message => message.SenderUserId).Distinct().ToArray(), cancellationToken);
        return messages.Select(message => Map(message, names)).ToArray();
    }

    public async Task MarkReadAsync(Guid connectionId, string userId, CancellationToken cancellationToken)
    {
        await RequireParticipantAsync(connectionId, userId, cancellationToken);
        await _chatRepository.MarkReadAsync(connectionId, userId.Trim(), Since(), DateTime.UtcNow, cancellationToken);
    }

    public Task<IReadOnlyList<ChatUnreadDto>> GetUnreadAsync(string userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("É necessário ter um usuário autenticado.", nameof(userId));

        return _chatRepository.GetUnreadAsync(userId.Trim(), Since(), cancellationToken);
    }

    private DateTime Since() => DateTime.UtcNow - _options.Retention;

    private async Task<VetConnection> RequireParticipantAsync(Guid connectionId, string userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("É necessário ter um usuário autenticado.", nameof(userId));

        var normalized = userId.Trim();
        var connection = connectionId == Guid.Empty ? null : await _connectionRepository.GetByIdAsync(connectionId, cancellationToken);
        var isParticipant = connection is not null
            && connection.Status == VetConnectionStatus.Active.Value
            && connection.TutorUserId is not null
            && (StringComparer.Ordinal.Equals(connection.VetUserId, normalized)
                || StringComparer.Ordinal.Equals(connection.TutorUserId, normalized));

        return isParticipant ? connection! : throw new VetConnectionNotFoundException();
    }

    private static ChatMessageDto Map(ChatMessage message, IReadOnlyDictionary<string, string> names) => new()
    {
        Id = message.Id,
        ConnectionId = message.ConnectionId,
        SenderUserId = message.SenderUserId,
        SenderName = names.GetValueOrDefault(message.SenderUserId) ?? string.Empty,
        Text = message.Text,
        SentAtUtc = message.SentAtUtc
    };
}