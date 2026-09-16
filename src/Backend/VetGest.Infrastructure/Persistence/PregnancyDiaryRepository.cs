using Microsoft.EntityFrameworkCore;
using VetGest.Application.Pregnancies;
using VetGest.Domain.Entities;

namespace VetGest.Infrastructure.Persistence;

public sealed class PregnancyDiaryRepository : IPregnancyDiaryRepository
{
    private readonly VetGestDbContext _dbContext;

    public PregnancyDiaryRepository(VetGestDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(PregnancyDiaryEntry entry, string ownerId, CancellationToken cancellationToken)
    {
        _dbContext.Entry(entry).Property("OwnerId").CurrentValue = ownerId;
        await _dbContext.PregnancyDiaryEntries.AddAsync(entry, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExistsForDateAsync(Guid pregnancyId, DateTime entryDate, string ownerId, CancellationToken cancellationToken) =>
        _dbContext.PregnancyDiaryEntries.AnyAsync(entry => entry.PregnancyId == pregnancyId &&
            entry.EntryDate == entryDate && EF.Property<string>(entry, "OwnerId") == ownerId, cancellationToken);

    public async Task<IReadOnlyList<PregnancyDiaryEntry>> ListOwnedAsync(Guid pregnancyId, string ownerId, CancellationToken cancellationToken) =>
        await _dbContext.PregnancyDiaryEntries
            .Where(entry => entry.PregnancyId == pregnancyId && EF.Property<string>(entry, "OwnerId") == ownerId)
            .OrderByDescending(entry => entry.EntryDate)
            .ThenByDescending(entry => EF.Property<DateTime>(entry, "CreatedAt"))
            .ToListAsync(cancellationToken);
}