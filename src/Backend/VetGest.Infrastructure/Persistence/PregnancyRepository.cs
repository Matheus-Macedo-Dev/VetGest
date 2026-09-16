using Microsoft.EntityFrameworkCore;
using VetGest.Application.Pregnancies;
using VetGest.Domain.Entities;

namespace VetGest.Infrastructure.Persistence;

public sealed class PregnancyRepository : IPregnancyRepository
{
    private readonly VetGestDbContext _dbContext;

    public PregnancyRepository(VetGestDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(Pregnancy pregnancy, string ownerId, CancellationToken cancellationToken)
    {
        _dbContext.Entry(pregnancy).Property("OwnerId").CurrentValue = ownerId;
        await _dbContext.Pregnancies.AddAsync(pregnancy, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Pregnancy?> GetOwnedAsync(Guid petId, Guid pregnancyId, string ownerId, CancellationToken cancellationToken) =>
        _dbContext.Pregnancies.SingleOrDefaultAsync(pregnancy => pregnancy.Id == pregnancyId &&
            pregnancy.PetId == petId && EF.Property<string>(pregnancy, "OwnerId") == ownerId, cancellationToken);

    public Task<Pregnancy?> GetCurrentOwnedAsync(Guid petId, string ownerId, CancellationToken cancellationToken) =>
        _dbContext.Pregnancies
            .Include(pregnancy => pregnancy.Pet)
            .Where(pregnancy => pregnancy.PetId == petId &&
                pregnancy.Status != "Completed" && pregnancy.Status != "Cancelled" &&
                EF.Property<string>(pregnancy, "OwnerId") == ownerId)
            .OrderByDescending(pregnancy => pregnancy.EstimatedDueDate)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<Phase?> GetPhaseForDayAsync(string species, int gestationalDay, CancellationToken cancellationToken) =>
        _dbContext.Phases
            .Where(phase => phase.Species == species &&
                phase.StartDayGestation <= gestationalDay && phase.EndDayGestation >= gestationalDay)
            .SingleOrDefaultAsync(cancellationToken);

    public Task<Phase?> GetLastPhaseAsync(string species, CancellationToken cancellationToken) =>
        _dbContext.Phases.Where(phase => phase.Species == species)
            .OrderByDescending(phase => phase.EndDayGestation)
            .FirstOrDefaultAsync(cancellationToken);
}