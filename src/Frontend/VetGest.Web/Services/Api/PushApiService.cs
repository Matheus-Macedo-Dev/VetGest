using System.Net.Http.Json;
using VetGest.Contracts.Push;

namespace VetGest.Web.Services.Api;

public sealed class PushApiService(HttpClient httpClient, IApiWakeUpService wakeUpService)
    : ApiClient(httpClient, wakeUpService)
{
    public Task<ApiCallResult<PushPublicKeyDto>> GetPublicKeyAsync(CancellationToken cancellationToken = default) =>
        SendAsync<PushPublicKeyDto>(new HttpRequestMessage(HttpMethod.Get, "api/push/public-key"), cancellationToken);

    public Task<ApiCallResult<bool>> SubscribeAsync(PushSubscribeRequest request, CancellationToken cancellationToken = default) =>
        SendAsync<bool>(
            new HttpRequestMessage(HttpMethod.Post, "api/push/subscribe") { Content = JsonContent.Create(request) },
            cancellationToken);

    public Task<ApiCallResult<bool>> UnsubscribeAsync(string endpoint, CancellationToken cancellationToken = default) =>
        SendAsync<bool>(
            new HttpRequestMessage(HttpMethod.Post, "api/push/unsubscribe") { Content = JsonContent.Create(new PushUnsubscribeRequest { Endpoint = endpoint }) },
            cancellationToken);
}