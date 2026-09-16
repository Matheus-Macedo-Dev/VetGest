using VetGest.Contracts.Pregnancies;

namespace VetGest.Web.Services.Api;

public sealed class CareContentService(HttpClient httpClient) : ApiClient(httpClient)
{
    public Task<ApiCallResult<CareContentDto>> GetAsync(Guid petId, Guid pregnancyId,
        CancellationToken cancellationToken = default) =>
        SendAsync<CareContentDto>(
            new HttpRequestMessage(HttpMethod.Get,
                $"api/pets/{petId}/pregnancies/{pregnancyId}/care"), cancellationToken);
}