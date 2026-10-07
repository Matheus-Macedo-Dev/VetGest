using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using VetGest.API.Tests.Infrastructure;

namespace VetGest.API.Tests;

public sealed class VetConnectionsHubAuthorizationTests : IClassFixture<VetGestApiFactory>
{
    private readonly VetGestApiFactory _factory;

    public VetConnectionsHubAuthorizationTests(VetGestApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Hub_connection_without_authentication_is_rejected()
    {
        var connection = BuildConnection();

        await Assert.ThrowsAnyAsync<Exception>(() => connection.StartAsync());

        await connection.DisposeAsync();
    }

    [Fact]
    public async Task Join_pregnancy_fails_for_unlinked_user_and_succeeds_for_linked_user()
    {
        var pregnancyId = Guid.NewGuid();
        var userId = "vet-1";

        var unlinkedConnection = BuildConnection($"{userId};Vet");
        await unlinkedConnection.StartAsync();
        await Assert.ThrowsAsync<HubException>(() => unlinkedConnection.InvokeAsync("JoinPregnancy", pregnancyId));
        await unlinkedConnection.DisposeAsync();

        _factory.VetConnectionService.AllowedPregnancyAccess.Add((pregnancyId, userId));
        var linkedConnection = BuildConnection($"{userId};Vet");
        await linkedConnection.StartAsync();
        await linkedConnection.InvokeAsync("JoinPregnancy", pregnancyId);
        await linkedConnection.DisposeAsync();
    }

    private HubConnection BuildConnection(string? bearerToken = null)
    {
        return new HubConnectionBuilder()
            .WithUrl(new Uri(_factory.Server.BaseAddress, "/hubs/vet-connections"), options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
                if (!string.IsNullOrWhiteSpace(bearerToken))
                    options.AccessTokenProvider = () => Task.FromResult<string?>(bearerToken);
            })
            .Build();
    }
}
