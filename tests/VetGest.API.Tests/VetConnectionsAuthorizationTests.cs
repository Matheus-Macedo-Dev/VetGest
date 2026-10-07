using System.Net;
using System.Net.Http.Json;
using VetGest.API.Tests.Infrastructure;
using VetGest.Application.Pregnancies;
using VetGest.Contracts.Common;
using VetGest.Contracts.Pregnancies;

namespace VetGest.API.Tests;

public sealed class VetConnectionsAuthorizationTests : IClassFixture<VetGestApiFactory>
{
    private readonly VetGestApiFactory _factory;

    public VetConnectionsAuthorizationTests(VetGestApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_invitation_requires_authenticated_vet_role()
    {
        var anonymousClient = _factory.CreateClient();
        var payload = new CreateVetInvitationRequest
        {
            PetId = Guid.NewGuid(),
            PregnancyId = Guid.NewGuid(),
            TutorUserId = "tutor-1"
        };

        var anonymousResponse = await anonymousClient.PostAsJsonAsync("/api/vet-connections/invitations", payload);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        var tutorClient = CreateClient("tutor-1", "Tutor");
        var tutorResponse = await tutorClient.PostAsJsonAsync("/api/vet-connections/invitations", payload);
        Assert.Equal(HttpStatusCode.Forbidden, tutorResponse.StatusCode);

        var vetClient = CreateClient("vet-1", "Vet");
        var vetResponse = await vetClient.PostAsJsonAsync("/api/vet-connections/invitations", payload);
        Assert.Equal(HttpStatusCode.Created, vetResponse.StatusCode);
    }

    [Fact]
    public async Task Accept_invitation_requires_tutor_role()
    {
        var vetClient = CreateClient("vet-1", "Vet");
        var vetResponse = await vetClient.PostAsJsonAsync("/api/vet-connections/accept", new AcceptVetInvitationRequest
        {
            InvitationCode = "INVITE123456"
        });

        Assert.Equal(HttpStatusCode.Forbidden, vetResponse.StatusCode);

        var tutorClient = CreateClient("tutor-1", "Tutor");
        var tutorResponse = await tutorClient.PostAsJsonAsync("/api/vet-connections/accept", new AcceptVetInvitationRequest
        {
            InvitationCode = "INVITE123456"
        });
        Assert.Equal(HttpStatusCode.OK, tutorResponse.StatusCode);
    }

    [Fact]
    public async Task Revoke_returns_forbid_for_non_participant()
    {
        var connectionId = Guid.NewGuid();
        _factory.VetConnectionService.OnRevoke = static (_, __) => throw new VetConnectionForbiddenException();

        var client = CreateClient("other-user", "Tutor");
        var response = await client.PostAsync($"/api/vet-connections/{connectionId}/revoke", content: null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task List_requires_authenticated_user()
    {
        var anonymousClient = _factory.CreateClient();
        var response = await anonymousClient.GetAsync("/api/vet-connections");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var authenticatedClient = CreateClient("tutor-1", "Tutor");
        var okResponse = await authenticatedClient.GetAsync("/api/vet-connections");
        Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);

        var envelope = await okResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<VetConnectionDto>>>();
        Assert.NotNull(envelope);
        Assert.True(envelope!.Success);
    }

    private HttpClient CreateClient(string userId, params string[] roles)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            $"{userId};{string.Join(',', roles)}");
        return client;
    }
}
