using VetGest.Contracts.Pregnancies;

namespace VetGest.Web.Services.Api;

public sealed class VetPatientService(HttpClient httpClient, IApiWakeUpService wakeUpService)
    : ApiClient(httpClient, wakeUpService)
{
    public Task<ApiCallResult<IReadOnlyList<VetPatientDto>>> ListAsync(CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<VetPatientDto>>(new HttpRequestMessage(HttpMethod.Get, "api/vet/patients"), cancellationToken);

    public Task<ApiCallResult<VetPatientDetailDto>> GetAsync(Guid connectionId, CancellationToken cancellationToken = default) =>
        SendAsync<VetPatientDetailDto>(new HttpRequestMessage(HttpMethod.Get, $"api/vet/patients/{connectionId}"), cancellationToken);
}
