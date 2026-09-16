using System.Net.Http.Json;
using VetGest.Contracts.Pregnancies;

namespace VetGest.Web.Services.Api;

public sealed class DiaryService(HttpClient httpClient) : ApiClient(httpClient)
{
    public Task<ApiCallResult<IReadOnlyList<DiaryEntryDto>>> GetEntriesAsync(Guid petId, Guid pregnancyId,
        CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<DiaryEntryDto>>(
            new HttpRequestMessage(HttpMethod.Get, Route(petId, pregnancyId)), cancellationToken);

    public Task<ApiCallResult<DiaryEntryDto>> CreateAsync(Guid petId, Guid pregnancyId,
        CreateDiaryEntryRequest request, CancellationToken cancellationToken = default)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, Route(petId, pregnancyId))
        {
            Content = JsonContent.Create(request)
        };
        return SendAsync<DiaryEntryDto>(message, cancellationToken);
    }

    private static string Route(Guid petId, Guid pregnancyId) =>
        $"api/pets/{petId}/pregnancies/{pregnancyId}/diary";
}