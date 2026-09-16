using Microsoft.EntityFrameworkCore;
using VetGest.Application.Pets;
using VetGest.Domain.Entities;

namespace VetGest.Infrastructure.Persistence;

public sealed class PetRepository : IPetRepository
{
    private readonly VetGestDbContext _dbContext;

    public PetRepository(VetGestDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(Pet pet, string ownerId, CancellationToken cancellationToken)
    {
        _dbContext.Entry(pet).Property("OwnerId").CurrentValue = ownerId;
        await _dbContext.Pets.AddAsync(pet, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Pet>> ListOwnedAsync(string ownerId, CancellationToken cancellationToken) =>
        await _dbContext.Pets
            .Where(p => EF.Property<string>(p, "OwnerId") == ownerId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

    public Task<Pet?> GetOwnedAsync(Guid petId, string ownerId, CancellationToken cancellationToken) =>
        _dbContext.Pets.SingleOrDefaultAsync(
            p => p.Id == petId && EF.Property<string>(p, "OwnerId") == ownerId,
            cancellationToken);
}