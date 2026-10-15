using System.Net;
using System.Net.Http.Json;
using VetGest.API.Tests.Infrastructure;
using VetGest.Contracts.Admin;

namespace VetGest.API.Tests;

public sealed class AdminVetsAuthorizationTests : IClassFixture<VetGestApiFactory>
{
    private static readonly CreateVetRequest ValidRequest = new()
    {
        FullName = "Vet Teste",
        Email = "vet.teste@example.com",
        CrmvNumber = "12345",
        CrmvState = "SP"
    };

    private readonly VetGestApiFactory _factory;

    public AdminVetsAuthorizationTests(VetGestApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Anonymous_requests_are_unauthorized()
    {
        var client = _factory.CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/admin/vets")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/admin/vets", ValidRequest)).StatusCode);
    }

    [Theory]
    [InlineData("Tutor")]
    [InlineData("Vet")]
    public async Task Non_admin_roles_are_forbidden_on_every_endpoint(string role)
    {
        var client = CreateClient("user-1", role);

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/admin/vets")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/admin/vets", ValidRequest)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync("/api/admin/vets/vet-1/setup-link", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync("/api/admin/vets/vet-1/activate", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync("/api/admin/vets/vet-1/deactivate", null)).StatusCode);
    }

    [Fact]
    public async Task Admin_can_manage_vets()
    {
        var client = CreateClient("admin-1", "Admin");

        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/admin/vets", ValidRequest)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/admin/vets")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync("/api/admin/vets/vet-1/setup-link", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync("/api/admin/vets/vet-1/deactivate", null)).StatusCode);
    }

    [Theory]
    [InlineData("Tutor")]
    [InlineData("Vet")]
    public async Task Non_admin_roles_are_forbidden_on_tutor_management(string role)
    {
        var client = CreateClient("user-1", role);

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/admin/tutors")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync("/api/admin/tutors/t-1/deactivate", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync("/api/admin/tutors/t-1/activate", null)).StatusCode);
    }

    [Fact]
    public async Task Anonymous_and_admin_access_to_tutor_management()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await _factory.CreateClient().GetAsync("/api/admin/tutors")).StatusCode);

        var admin = CreateClient("admin-1", "Admin");
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync("/api/admin/tutors")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync("/api/admin/tutors/t-1/deactivate", null)).StatusCode);
    }

    private HttpClient CreateClient(string userId, params string[] roles)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", $"{userId};{string.Join(',', roles)}");
        return client;
    }
}
