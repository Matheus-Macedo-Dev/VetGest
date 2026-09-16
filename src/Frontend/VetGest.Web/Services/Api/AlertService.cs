using VetGest.Contracts.Pregnancies;

namespace VetGest.Web.Services.Api;

public sealed class AlertService(HttpClient httpClient) : ApiClient(httpClient)
{
    public Task<ApiCallResult<TodayAlertsDto>> GetAsync(Guid petId, Guid pregnancyId,
        CancellationToken cancellationToken = default) =>
        SendAsync<TodayAlertsDto>(
            new HttpRequestMessage(HttpMethod.Get,
                $"api/pets/{petId}/pregnancies/{pregnancyId}/alerts"), cancellationToken);
}
