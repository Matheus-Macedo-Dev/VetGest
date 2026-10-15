using System.Text.Json;
using Microsoft.Extensions.Logging;
using VetGest.Application.Push;
using WebPush;

namespace VetGest.Infrastructure.Persistence;

public sealed class WebPushSender : IPushSender
{
    private readonly IPushSubscriptionRepository _repository;
    private readonly PushOptions _options;
    private readonly ILogger<WebPushSender> _logger;

    public WebPushSender(IPushSubscriptionRepository repository, PushOptions options, ILogger<WebPushSender> logger)
    {
        _repository = repository;
        _options = options;
        _logger = logger;
    }

    public bool Enabled => _options.Enabled;

    public async Task SendAsync(string userId, PushMessage message, CancellationToken cancellationToken)
    {
        if (!Enabled)
            return;

        var subscriptions = await _repository.ListByUserAsync(userId, cancellationToken);
        if (subscriptions.Count == 0)
            return;

        var vapid = new VapidDetails(_options.Subject, _options.PublicKey, _options.PrivateKey);
        var payload = JsonSerializer.Serialize(new { title = message.Title, body = message.Body, url = message.Url });
        using var client = new WebPushClient();

        foreach (var subscription in subscriptions)
        {
            try
            {
                await client.SendNotificationAsync(
                    new PushSubscription(subscription.Endpoint, subscription.P256dh, subscription.Auth), payload, vapid, cancellationToken);
            }
            catch (WebPushException exception) when (exception.StatusCode is System.Net.HttpStatusCode.NotFound or System.Net.HttpStatusCode.Gone)
            {
                await _repository.RemoveByEndpointAsync(subscription.Endpoint, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Push delivery failed for a subscription of user {UserId}.", userId);
            }
        }
    }
}