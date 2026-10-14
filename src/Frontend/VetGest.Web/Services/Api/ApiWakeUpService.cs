using System.Net;

namespace VetGest.Web.Services.Api;

public sealed class ApiWakeUpService(HttpClient httpClient, ILogger<ApiWakeUpService> logger)
    : IApiWakeUpService
{
    private static readonly TimeSpan WakeUpTimeout = TimeSpan.FromSeconds(30);

    public async Task WakeAsync(CancellationToken cancellationToken)
    {
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(WakeUpTimeout);

        try
        {
            using var response = await httpClient.GetAsync(
                "health",
                HttpCompletionOption.ResponseHeadersRead,
                timeoutSource.Token);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("API wake-up request returned HTTP {StatusCode}.", response.StatusCode);
            }
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "API wake-up request failed; continuing with the original request.");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("API wake-up request timed out after {TimeoutSeconds} seconds.",
                WakeUpTimeout.TotalSeconds);
        }
    }
}
