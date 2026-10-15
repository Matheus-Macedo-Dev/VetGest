using Microsoft.EntityFrameworkCore;
using VetGest.Application.Chat;
using VetGest.Contracts.Chat;
using VetGest.Domain.Entities;
using VetGest.Domain.ValueObjects;

namespace VetGest.Infrastructure.Persistence;

public sealed class ChatRepository : IChatRepository
{
    private readonly VetGestDbContext _dbContext;

    public ChatRepository(VetGestDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(ChatMessage message, CancellationToken cancellationToken)
    {
        await _dbContext.ChatMessages.AddAsync(message, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ChatMessage>> ListAsync(Guid connectionId, DateTime sinceUtc, CancellationToken cancellationToken) =>
        await _dbContext.ChatMessages
            .AsNoTracking()
            .Where(message => message.ConnectionId == connectionId && message.SentAtUtc >= sinceUtc)
            .OrderBy(message => message.SentAtUtc)
            .ToListAsync(cancellationToken);

    public async Task MarkReadAsync(Guid connectionId, string readerUserId, DateTime sinceUtc, DateTime nowUtc, CancellationToken cancellationToken) =>
        await _dbContext.ChatMessages
            .Where(message => message.ConnectionId == connectionId
                && message.SenderUserId != readerUserId
                && message.ReadAtUtc == null
                && message.SentAtUtc >= sinceUtc)
            .ExecuteUpdateAsync(setters => setters.SetProperty(message => message.ReadAtUtc, nowUtc), cancellationToken);

    public async Task<IReadOnlyList<ChatUnreadDto>> GetUnreadAsync(string userId, DateTime sinceUtc, CancellationToken cancellationToken)
    {
        var active = VetConnectionStatus.Active.Value;
        var rows = await (
            from message in _dbContext.ChatMessages
            join connection in _dbContext.VetConnections on message.ConnectionId equals connection.Id
            where connection.Status == active
                && (connection.VetUserId == userId || connection.TutorUserId == userId)
                && message.SenderUserId != userId
                && message.ReadAtUtc == null
                && message.SentAtUtc >= sinceUtc
            group message by message.ConnectionId into grouped
            select new { ConnectionId = grouped.Key, Count = grouped.Count() })
            .ToListAsync(cancellationToken);

        return rows.Select(row => new ChatUnreadDto { ConnectionId = row.ConnectionId, UnreadCount = row.Count }).ToArray();
    }

    public async Task<int> DeleteExpiredAsync(DateTime cutoffUtc, CancellationToken cancellationToken)
    {
        var active = VetConnectionStatus.Active.Value;
        return await _dbContext.ChatMessages
            .Where(message => message.SentAtUtc < cutoffUtc
                || !_dbContext.VetConnections.Any(connection => connection.Id == message.ConnectionId && connection.Status == active))
            .ExecuteDeleteAsync(cancellationToken);
    }
}