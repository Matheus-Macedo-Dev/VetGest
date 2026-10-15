using VetGest.Contracts.Admin;

namespace VetGest.Web.Services.Api;

public sealed class AdminTutorService(HttpClient httpClient, IApiWakeUpService wakeUpService)
    : ApiClient(httpClient, wakeUpService)
{
    public Task<ApiCallResult<IReadOnlyList<AdminTutorDto>>> ListAsync(CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<AdminTutorDto>>(new HttpRequestMessage(HttpMethod.Get, "api/admin/tutors"), cancellationToken);

    public Task<ApiCallResult<AdminTutorDto>> SetActiveAsync(string tutorUserId, bool isActive, CancellationToken cancellationToken = default) =>
        SendAsync<AdminTutorDto>(
            new HttpRequestMessage(HttpMethod.Post,
                $"api/admin/tutors/{Uri.EscapeDataString(tutorUserId)}/{(isActive ? "activate" : "deactivate")}"),
            cancellationToken);
}