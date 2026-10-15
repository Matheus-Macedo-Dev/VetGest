using VetGest.Application.Pregnancies;
using VetGest.Contracts.Push;

namespace VetGest.Application.Push;

public sealed class PushSubscriptionService : IPushSubscriptionService
{
    private readonly IPushSubscriptionRepository _repository;
    private readonly PushOptions _options;

    public PushSubscriptionService(IPushSubscriptionRepository repository, PushOptions options)
    {
        _repository = repository;
        _options = options;
    }

    public PushPublicKeyDto GetPublicKey() => new()
    {
        Enabled = _options.Enabled,
        PublicKey = _options.Enabled ? _options.PublicKey! : string.Empty
    };

    public Task SubscribeAsync(string userId, PushSubscribeRequest request, CancellationToken cancellationToken)
    {
        EnsureUser(userId);
        if (!PushEndpointPolicy.IsAllowed(request.Endpoint))
        {
            throw new VetConnectionValidationException(new Dictionary<string, string[]>
            {
                [nameof(request.Endpoint)] = ["Endpoint de notificação não suportado."]
            });
        }

        if (string.IsNullOrWhiteSpace(request.P256dh) || string.IsNullOrWhiteSpace(request.Auth))
        {
            throw new VetConnectionValidationException(new Dictionary<string, string[]>
            {
                [nameof(request.P256dh)] = ["Chaves da assinatura são obrigatórias."]
            });
        }

        return _repository.UpsertAsync(userId.Trim(), request.Endpoint, request.P256dh, request.Auth, cancellationToken);
    }

    public Task UnsubscribeAsync(string userId, PushUnsubscribeRequest request, CancellationToken cancellationToken)
    {
        EnsureUser(userId);
        return _repository.RemoveAsync(userId.Trim(), request.Endpoint, cancellationToken);
    }

    private static void EnsureUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("É necessário ter um usuário autenticado.", nameof(userId));
    }
}