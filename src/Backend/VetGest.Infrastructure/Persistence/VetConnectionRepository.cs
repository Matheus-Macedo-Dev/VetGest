using Microsoft.EntityFrameworkCore;
using VetGest.Application.Pregnancies;
using VetGest.Domain.Entities;
using VetGest.Domain.ValueObjects;

namespace VetGest.Infrastructure.Persistence;

public sealed class VetConnectionRepository : IVetConnectionRepository
{
    private readonly VetGestDbContext _dbContext;

    public VetConnectionRepository(VetGestDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> InvitationCodeExistsAsync(string invitationCode, CancellationToken cancellationToken) =>
        _dbContext.VetConnections.AnyAsync(
            connection => connection.InvitationCode == invitationCode.ToUpperInvariant(),
            cancellationToken);

    public async Task AddAsync(VetConnection connection, string ownerId, CancellationToken cancellationToken)
    {
        _dbContext.Entry(connection).Property("OwnerId").CurrentValue = ownerId;
        await _dbContext.VetConnections.AddAsync(connection, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Pregnancy?> GetOwnedPregnancyAsync(
        Guid petId,
        Guid pregnancyId,
        string ownerId,
        CancellationToken cancellationToken) =>
        _dbContext.Pregnancies.SingleOrDefaultAsync(
            pregnancy => pregnancy.Id == pregnancyId
                && pregnancy.PetId == petId
                && EF.Property<string>(pregnancy, "OwnerId") == ownerId,
            cancellationToken);

    public Task<VetConnection?> GetByInvitationCodeAsync(string invitationCode, CancellationToken cancellationToken) =>
        _dbContext.VetConnections
            .Include(connection => connection.Pregnancy)
            .SingleOrDefaultAsync(
            connection => connection.InvitationCode == invitationCode.ToUpperInvariant(),
            cancellationToken);

    public Task<VetConnection?> GetByIdAsync(Guid connectionId, CancellationToken cancellationToken) =>
        _dbContext.VetConnections
            .Include(connection => connection.Pregnancy)
            .SingleOrDefaultAsync(connection => connection.Id == connectionId, cancellationToken);

    public async Task<IReadOnlyList<VetConnection>> ListByUserAsync(string userId, CancellationToken cancellationToken)
    {
        return await _dbContext.VetConnections
            .Include(connection => connection.Pregnancy)
            .Where(connection => connection.VetUserId == userId || connection.TutorUserId == userId)
            .OrderByDescending(connection => connection.AcceptedAt ?? connection.InvitationExpiresAt)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Guid?> GetPetIdByPregnancyIdAsync(Guid pregnancyId, CancellationToken cancellationToken)
    {
        return await _dbContext.Pregnancies
            .Where(pregnancy => pregnancy.Id == pregnancyId)
            .Select(pregnancy => (Guid?)pregnancy.PetId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> CanUserAccessPregnancyAsync(Guid pregnancyId, string userId, CancellationToken cancellationToken)
    {
        var isOwner = await _dbContext.Pregnancies.AnyAsync(
            pregnancy => pregnancy.Id == pregnancyId && EF.Property<string>(pregnancy, "OwnerId") == userId,
            cancellationToken);
        if (isOwner)
            return true;

        return await _dbContext.VetConnections.AnyAsync(
            connection => connection.PregnancyId == pregnancyId
                && connection.VetUserId == userId
                && connection.Status == VetConnectionStatus.Active.Value,
            cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
