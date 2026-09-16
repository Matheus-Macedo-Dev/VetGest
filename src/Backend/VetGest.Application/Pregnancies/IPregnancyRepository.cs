using VetGest.Domain.Entities;

namespace VetGest.Application.Pregnancies;

public interface IPregnancyRepository
{
    Task AddAsync(Pregnancy pregnancy, string ownerId, CancellationToken cancellationToken);
    Task<Pregnancy?> GetOwnedAsync(Guid petId, Guid pregnancyId, string ownerId, CancellationToken cancellationToken);
    Task<Pregnancy?> GetCurrentOwnedAsync(Guid petId, string ownerId, CancellationToken cancellationToken);
    Task<Phase?> GetPhaseForDayAsync(string species, int gestationalDay, CancellationToken cancellationToken);
    Task<Phase?> GetLastPhaseAsync(string species, CancellationToken cancellationToken);
}