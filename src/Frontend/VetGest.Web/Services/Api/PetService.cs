using System.Net.Http.Json;
using VetGest.Contracts.Common;
using VetGest.Contracts.Pets;

namespace VetGest.Web.Services.Api;

public sealed class PetService(HttpClient httpClient) : ApiClient(httpClient)
{
    public Task<ApiCallResult<IReadOnlyList<PetDto>>> GetPetsAsync(CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<PetDto>>(new HttpRequestMessage(HttpMethod.Get, "api/pets"), cancellationToken);

    public Task<ApiCallResult<PetDto>> CreateAsync(CreatePetRequest request, CancellationToken cancellationToken = default)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, "api/pets")
        {
            Content = JsonContent.Create(request)
        };
        return SendAsync<PetDto>(message, cancellationToken);
    }
}