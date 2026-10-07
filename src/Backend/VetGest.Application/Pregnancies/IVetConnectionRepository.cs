using VetGest.Domain.Entities;

namespace VetGest.Application.Pregnancies;

public interface IVetConnectionRepository
{
    Task<bool> InvitationCodeExistsAsync(string invitationCode, CancellationToken cancellationToken);
    Task AddAsync(VetConnection connection, string ownerId, CancellationToken cancellationToken);
    Task<Pregnancy?> GetOwnedPregnancyAsync(Guid petId, Guid pregnancyId, string ownerId, CancellationToken cancellationToken);
    Task<VetConnection?> GetByInvitationCodeAsync(string invitationCode, CancellationToken cancellationToken);
    Task<VetConnection?> GetByIdAsync(Guid connectionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<VetConnection>> ListByUserAsync(string userId, CancellationToken cancellationToken);
    Task<Guid?> GetPetIdByPregnancyIdAsync(Guid pregnancyId, CancellationToken cancellationToken);
    Task<bool> CanUserAccessPregnancyAsync(Guid pregnancyId, string userId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
