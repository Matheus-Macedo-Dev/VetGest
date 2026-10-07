using System.Net.Http.Json;
using VetGest.Contracts.Pregnancies;

namespace VetGest.Web.Services.Api;

public sealed class VetConnectionService(HttpClient httpClient) : ApiClient(httpClient)
{
    public Task<ApiCallResult<IReadOnlyList<VetConnectionDto>>> ListAsync(CancellationToken cancellationToken = default) =>
        SendAsync<IReadOnlyList<VetConnectionDto>>(
            new HttpRequestMessage(HttpMethod.Get, "api/vet-connections"),
            cancellationToken);

    public Task<ApiCallResult<VetConnectionDto>> CreateInvitationAsync(
        CreateVetInvitationRequest request,
        CancellationToken cancellationToken = default)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, "api/vet-connections/invitations")
        {
            Content = JsonContent.Create(request)
        };
        return SendAsync<VetConnectionDto>(message, cancellationToken);
    }

    public Task<ApiCallResult<VetConnectionDto>> AcceptInvitationAsync(
        AcceptVetInvitationRequest request,
        CancellationToken cancellationToken = default)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, "api/vet-connections/accept")
        {
            Content = JsonContent.Create(request)
        };
        return SendAsync<VetConnectionDto>(message, cancellationToken);
    }

    public Task<ApiCallResult<VetConnectionDto>> RevokeAsync(Guid connectionId, CancellationToken cancellationToken = default) =>
        SendAsync<VetConnectionDto>(
            new HttpRequestMessage(HttpMethod.Post, $"api/vet-connections/{connectionId}/revoke"),
            cancellationToken);
}
