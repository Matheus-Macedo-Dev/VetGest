using VetGest.Application.Pregnancies;
using VetGest.Contracts.Pregnancies;

namespace VetGest.API.Tests.Infrastructure;

public sealed class TestVetPatientService : IVetPatientService
{
    public HashSet<(Guid ConnectionId, string VetUserId)> Allowed { get; } = new();

    public Task<IReadOnlyList<VetPatientDto>> ListAsync(string vetUserId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<VetPatientDto>>([]);

    public Task<VetPatientDetailDto> GetDetailAsync(Guid connectionId, string vetUserId, CancellationToken cancellationToken)
    {
        if (!Allowed.Contains((connectionId, vetUserId)))
            throw new VetConnectionNotFoundException();

        return Task.FromResult(new VetPatientDetailDto { Patient = new VetPatientDto { ConnectionId = connectionId } });
    }
}

public sealed class TestUserDirectory : IUserDirectory
{
    public Task<IReadOnlyDictionary<string, string>> GetDisplayNamesAsync(
        IReadOnlyCollection<string> userIds, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyDictionary<string, string>>(userIds.ToDictionary(id => id, id => $"Name {id}"));
}
