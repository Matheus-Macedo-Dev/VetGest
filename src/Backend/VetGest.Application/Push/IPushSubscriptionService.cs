using VetGest.Contracts.Push;

namespace VetGest.Application.Push;

public interface IPushSubscriptionService
{
    PushPublicKeyDto GetPublicKey();

    /// <summary>Throws <see cref="Pregnancies.VetConnectionValidationException"/> for invalid or disallowed endpoints.</summary>
    Task SubscribeAsync(string userId, PushSubscribeRequest request, CancellationToken cancellationToken);

    Task UnsubscribeAsync(string userId, PushUnsubscribeRequest request, CancellationToken cancellationToken);
}