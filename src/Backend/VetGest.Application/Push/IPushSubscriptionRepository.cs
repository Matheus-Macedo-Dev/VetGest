using VetGest.Domain.Entities;

namespace VetGest.Application.Push;

public interface IPushSubscriptionRepository
{
    Task UpsertAsync(string userId, string endpoint, string p256dh, string auth, CancellationToken cancellationToken);

    Task RemoveAsync(string userId, string endpoint, CancellationToken cancellationToken);

    Task RemoveByEndpointAsync(string endpoint, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserPushSubscription>> ListByUserAsync(string userId, CancellationToken cancellationToken);
}