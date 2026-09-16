using System.Net.Http.Json;
using VetGest.Contracts.Pregnancies;

namespace VetGest.Web.Services.Api;

public sealed class PregnancyService(HttpClient httpClient) : ApiClient(httpClient)
{
    public Task<ApiCallResult<PregnancyDto>> GetCurrentAsync(Guid petId,
        CancellationToken cancellationToken = default) =>
        SendAsync<PregnancyDto>(
            new HttpRequestMessage(HttpMethod.Get, $"api/pets/{petId}/pregnancies/current"), cancellationToken);

    public Task<ApiCallResult<PregnancyDto>> CreateAsync(Guid petId, CreatePregnancyRequest request,
        CancellationToken cancellationToken = default)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, $"api/pets/{petId}/pregnancies")
        {
            Content = JsonContent.Create(request)
        };
        return SendAsync<PregnancyDto>(message, cancellationToken);
    }
}