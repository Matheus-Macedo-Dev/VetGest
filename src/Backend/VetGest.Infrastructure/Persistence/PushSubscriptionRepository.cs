using Microsoft.EntityFrameworkCore;
using VetGest.Application.Push;
using VetGest.Domain.Entities;

namespace VetGest.Infrastructure.Persistence;

public sealed class PushSubscriptionRepository : IPushSubscriptionRepository
{
    private readonly VetGestDbContext _dbContext;

    public PushSubscriptionRepository(VetGestDbContext dbContext) => _dbContext = dbContext;

    public async Task UpsertAsync(string userId, string endpoint, string p256dh, string auth, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.PushSubscriptions.SingleOrDefaultAsync(item => item.Endpoint == endpoint, cancellationToken);
        if (existing is null)
            await _dbContext.PushSubscriptions.AddAsync(
                new UserPushSubscription(Guid.NewGuid(), userId, endpoint, p256dh, auth, DateTime.UtcNow), cancellationToken);
        else
            existing.Reassign(userId, p256dh, auth);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task RemoveAsync(string userId, string endpoint, CancellationToken cancellationToken) =>
        _dbContext.PushSubscriptions
            .Where(item => item.UserId == userId && item.Endpoint == endpoint)
            .ExecuteDeleteAsync(cancellationToken);

    public Task RemoveByEndpointAsync(string endpoint, CancellationToken cancellationToken) =>
        _dbContext.PushSubscriptions.Where(item => item.Endpoint == endpoint).ExecuteDeleteAsync(cancellationToken);

    public async Task<IReadOnlyList<UserPushSubscription>> ListByUserAsync(string userId, CancellationToken cancellationToken) =>
        await _dbContext.PushSubscriptions.AsNoTracking().Where(item => item.UserId == userId).ToListAsync(cancellationToken);
}