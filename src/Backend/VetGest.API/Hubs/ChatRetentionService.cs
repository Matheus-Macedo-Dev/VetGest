using VetGest.Application.Chat;

namespace VetGest.API.Hubs;

/// <summary>
/// Periodically deletes chat messages older than the retention window and messages of ended connections.
/// </summary>
public sealed class ChatRetentionService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(10);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ChatServiceOptions _options;
    private readonly ILogger<ChatRetentionService> _logger;

    public ChatRetentionService(IServiceScopeFactory scopeFactory, ChatServiceOptions options, ILogger<ChatRetentionService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IChatRepository>();
                await repository.DeleteExpiredAsync(DateTime.UtcNow - _options.Retention, stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogWarning(exception, "Chat retention cleanup failed; it will retry on the next cycle.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}