using Microsoft.AspNetCore.SignalR;
using VetGest.Contracts.Chat;
using VetGest.Contracts.Pregnancies;

namespace VetGest.API.Hubs;

public interface IVetConnectionRealtimePublisher
{
    Task PublishConnectionUpdatedAsync(VetConnectionDto connection, CancellationToken cancellationToken);
}

public sealed class VetConnectionRealtimePublisher : IVetConnectionRealtimePublisher
{
    private readonly IHubContext<VetConnectionsHub> _hubContext;
    private readonly IHubContext<ChatHub> _chatHubContext;

    public VetConnectionRealtimePublisher(IHubContext<VetConnectionsHub> hubContext, IHubContext<ChatHub> chatHubContext)
    {
        _hubContext = hubContext;
        _chatHubContext = chatHubContext;
    }

    public async Task PublishConnectionUpdatedAsync(VetConnectionDto connection, CancellationToken cancellationToken)
    {
        if (connection.PregnancyId == Guid.Empty)
            return;

        if (connection.Status == "Revoked")
            await _chatHubContext.Clients.Group(ChatHubGroups.Connection(connection.Id))
                .SendAsync(ChatHubMethods.ChatClosed, connection.Id, cancellationToken);

        var message = new VetConnectionUpdatedEvent
        {
            ConnectionId = connection.Id,
            PetId = connection.PetId,
            PregnancyId = connection.PregnancyId,
            Status = connection.Status,
            OccurredAtUtc = DateTime.UtcNow
        };

        await _hubContext.Clients
            .Group(VetConnectionHubGroups.Pregnancy(connection.PregnancyId))
            .SendAsync(VetConnectionHubMethods.ConnectionUpdated, message, cancellationToken);
    }
}
