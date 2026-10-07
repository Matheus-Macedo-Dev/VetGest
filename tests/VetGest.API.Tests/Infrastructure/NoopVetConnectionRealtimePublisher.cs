using VetGest.API.Hubs;
using VetGest.Contracts.Pregnancies;

namespace VetGest.API.Tests.Infrastructure;

public sealed class NoopVetConnectionRealtimePublisher : IVetConnectionRealtimePublisher
{
    public Task PublishConnectionUpdatedAsync(VetConnectionDto connection, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
