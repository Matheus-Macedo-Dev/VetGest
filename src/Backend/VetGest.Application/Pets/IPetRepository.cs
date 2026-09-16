using VetGest.Domain.Entities;

namespace VetGest.Application.Pets;

public interface IPetRepository
{
    Task AddAsync(Pet pet, string ownerId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Pet>> ListOwnedAsync(string ownerId, CancellationToken cancellationToken);
    Task<Pet?> GetOwnedAsync(Guid petId, string ownerId, CancellationToken cancellationToken);
}