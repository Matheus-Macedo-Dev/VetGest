using VetGest.Contracts.Pregnancies;

namespace VetGest.Web.Services.Api;

public sealed class ExaminationService(HttpClient httpClient) : ApiClient(httpClient)
{
    public Task<ApiCallResult<IReadOnlyList<ExaminationReminderDto>>> GetAsync(Guid petId, Guid pregnancyId,
        CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<ExaminationReminderDto>>(
            new HttpRequestMessage(HttpMethod.Get,
                $"api/pets/{petId}/pregnancies/{pregnancyId}/exams"), cancellationToken);
}