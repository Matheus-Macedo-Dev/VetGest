using VetGest.Contracts.Chat;

namespace VetGest.Application.Chat;

public interface IChatService
{
    /// <summary>Persists a message. Throws <see cref="Pregnancies.VetConnectionNotFoundException"/> unless the sender is a participant of an Active connection.</summary>
    Task<ChatSendResult> SendAsync(Guid connectionId, string senderUserId, string text, CancellationToken cancellationToken);

    Task<IReadOnlyList<ChatMessageDto>> GetHistoryAsync(Guid connectionId, string userId, CancellationToken cancellationToken);

    Task MarkReadAsync(Guid connectionId, string userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ChatUnreadDto>> GetUnreadAsync(string userId, CancellationToken cancellationToken);
}