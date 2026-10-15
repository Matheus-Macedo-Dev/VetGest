using VetGest.Contracts.Chat;
using VetGest.Domain.Entities;

namespace VetGest.Application.Chat;

public interface IChatRepository
{
    Task AddAsync(ChatMessage message, CancellationToken cancellationToken);

    Task<IReadOnlyList<ChatMessage>> ListAsync(Guid connectionId, DateTime sinceUtc, CancellationToken cancellationToken);

    Task MarkReadAsync(Guid connectionId, string readerUserId, DateTime sinceUtc, DateTime nowUtc, CancellationToken cancellationToken);

    Task<IReadOnlyList<ChatUnreadDto>> GetUnreadAsync(string userId, DateTime sinceUtc, CancellationToken cancellationToken);

    /// <summary>Deletes messages older than the cutoff and messages of connections that are no longer active.</summary>
    Task<int> DeleteExpiredAsync(DateTime cutoffUtc, CancellationToken cancellationToken);
}