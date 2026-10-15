using System.Net.Http.Json;
using VetGest.Contracts.Admin;

namespace VetGest.Web.Services.Api;

public sealed class AdminVetService(HttpClient httpClient, IApiWakeUpService wakeUpService)
    : ApiClient(httpClient, wakeUpService)
{
    public Task<ApiCallResult<IReadOnlyList<AdminVetDto>>> ListAsync(CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<AdminVetDto>>(new HttpRequestMessage(HttpMethod.Get, "api/admin/vets"), cancellationToken);

    public Task<ApiCallResult<CreatedVetDto>> CreateAsync(CreateVetRequest request, CancellationToken cancellationToken = default)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, "api/admin/vets") { Content = JsonContent.Create(request) };
        return SendAsync<CreatedVetDto>(message, cancellationToken);
    }

    public Task<ApiCallResult<VetSetupLinkDto>> IssueSetupLinkAsync(string vetUserId, CancellationToken cancellationToken = default) =>
        SendAsync<VetSetupLinkDto>(
            new HttpRequestMessage(HttpMethod.Post, $"api/admin/vets/{Uri.EscapeDataString(vetUserId)}/setup-link"),
            cancellationToken);

    public Task<ApiCallResult<AdminVetDto>> SetActiveAsync(string vetUserId, bool isActive, CancellationToken cancellationToken = default) =>
        SendAsync<AdminVetDto>(
            new HttpRequestMessage(HttpMethod.Post,
                $"api/admin/vets/{Uri.EscapeDataString(vetUserId)}/{(isActive ? "activate" : "deactivate")}"),
            cancellationToken);
}
