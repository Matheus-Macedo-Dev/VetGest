using Microsoft.AspNetCore.SignalR;
using VetGest.Contracts.Pregnancies;

namespace VetGest.API.Hubs;

public interface IVetConnectionRealtimePublisher
{
    Task PublishConnectionUpdatedAsync(VetConnectionDto connection, CancellationToken cancellationToken);
}

public sealed class VetConnectionRealtimePublisher : IVetConnectionRealtimePublisher
{
    private readonly IHubContext<VetConnectionsHub> _hubContext;

    public VetConnectionRealtimePublisher(IHubContext<VetConnectionsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task PublishConnectionUpdatedAsync(VetConnectionDto connection, CancellationToken cancellationToken)
    {
        var message = new VetConnectionUpdatedEvent
        {
            ConnectionId = connection.Id,
            PetId = connection.PetId,
            PregnancyId = connection.PregnancyId,
            Status = connection.Status,
            OccurredAtUtc = DateTime.UtcNow
        };

        return _hubContext.Clients
            .Group(VetConnectionHubGroups.Pregnancy(connection.PregnancyId))
            .SendAsync(VetConnectionHubMethods.ConnectionUpdated, message, cancellationToken);
    }
}
