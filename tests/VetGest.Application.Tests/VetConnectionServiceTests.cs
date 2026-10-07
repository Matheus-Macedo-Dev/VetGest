using VetGest.Application.Pregnancies;
using VetGest.Contracts.Pregnancies;
using VetGest.Domain.Entities;
using VetGest.Domain.ValueObjects;

namespace VetGest.Application.Tests;

public sealed class VetConnectionServiceTests
{
    [Fact]
    public async Task Creates_invitation_for_owned_pregnancy()
    {
        var pregnancy = CreatePregnancy();
        var repository = new FakeRepository(pregnancy, "tutor-1");
        var service = new VetConnectionService(repository);

        var result = await service.CreateInvitationAsync(new CreateVetInvitationRequest
        {
            PetId = pregnancy.PetId,
            PregnancyId = pregnancy.Id,
            TutorUserId = "tutor-1"
        }, "vet-1", CancellationToken.None);

        Assert.Equal("vet-1", result.VetUserId);
        Assert.Equal("tutor-1", result.TutorUserId);
        Assert.Equal(VetConnectionStatus.Pending.Value, result.Status);
        Assert.NotEqual(Guid.Empty, repository.Connection!.Id);
        Assert.Equal("tutor-1", repository.LastOwnerId);
        Assert.Equal(pregnancy.PetId, result.PetId);
    }

    [Fact]
    public async Task Clamps_ttl_to_maximum_when_configuration_exceeds_limit()
    {
        var pregnancy = CreatePregnancy();
        var repository = new FakeRepository(pregnancy, "tutor-1");
        var service = new VetConnectionService(repository, new VetConnectionServiceOptions
        {
            InvitationTtl = TimeSpan.FromDays(30),
            MaxInvitationTtl = TimeSpan.FromDays(10)
        });

        var result = await service.CreateInvitationAsync(new CreateVetInvitationRequest
        {
            PetId = pregnancy.PetId,
            PregnancyId = pregnancy.Id,
            TutorUserId = "tutor-1"
        }, "vet-1", CancellationToken.None);

        var ttl = result.InvitationExpiresAt - DateTime.UtcNow;
        Assert.True(ttl <= TimeSpan.FromDays(10).Add(TimeSpan.FromMinutes(1)));
    }

    [Fact]
    public async Task Rejects_invitation_when_pregnancy_is_missing_or_user_invalid()
    {
        var service = new VetConnectionService(new FakeRepository(null, "tutor-1"));

        await Assert.ThrowsAsync<VetConnectionValidationException>(() => service.CreateInvitationAsync(
            new CreateVetInvitationRequest(), "vet-1", CancellationToken.None));

        await Assert.ThrowsAsync<VetConnectionNotFoundException>(() => service.CreateInvitationAsync(
            new CreateVetInvitationRequest
            {
                PetId = Guid.NewGuid(),
                PregnancyId = Guid.NewGuid(),
                TutorUserId = "tutor-1"
            }, "vet-1", CancellationToken.None));
    }

    [Fact]
    public async Task Accepts_pending_invitation_only_for_target_tutor()
    {
        var pregnancy = CreatePregnancy();
        var repository = new FakeRepository(pregnancy, "tutor-1");
        var connection = CreatePendingConnection(pregnancy.Id, "vet-1", "tutor-1", "ABC123456789");
        repository.Connection = connection;
        var service = new VetConnectionService(repository);

        var accepted = await service.AcceptInvitationAsync(
            new AcceptVetInvitationRequest { InvitationCode = "ABC123456789" },
            "tutor-1",
            CancellationToken.None);

        Assert.Equal(VetConnectionStatus.Active.Value, accepted.Status);
        Assert.NotNull(accepted.AcceptedAt);
    Assert.Equal(pregnancy.PetId, accepted.PetId);

        await Assert.ThrowsAsync<VetConnectionNotFoundException>(() => service.AcceptInvitationAsync(
            new AcceptVetInvitationRequest { InvitationCode = "ABC123456789" },
            "tutor-2",
            CancellationToken.None));
    }

    [Fact]
    public async Task Accepts_invitation_code_case_insensitively()
    {
        var pregnancy = CreatePregnancy();
        var repository = new FakeRepository(pregnancy, "tutor-1");
        repository.Connection = CreatePendingConnection(pregnancy.Id, "vet-1", "tutor-1", "ABC123456789");
        var service = new VetConnectionService(repository);

        var accepted = await service.AcceptInvitationAsync(
            new AcceptVetInvitationRequest { InvitationCode = "abc123456789" },
            "tutor-1",
            CancellationToken.None);

        Assert.Equal(VetConnectionStatus.Active.Value, accepted.Status);
    }

    [Fact]
    public async Task Rejects_reused_or_revoked_invitation()
    {
        var pregnancy = CreatePregnancy();
        var repository = new FakeRepository(pregnancy, "tutor-1");
        var service = new VetConnectionService(repository);

        var active = CreatePendingConnection(pregnancy.Id, "vet-1", "tutor-1", "ACTIVE000001");
        active.Accept();
        repository.Connection = active;
        await Assert.ThrowsAsync<VetInvitationConflictException>(() => service.AcceptInvitationAsync(
            new AcceptVetInvitationRequest { InvitationCode = "ACTIVE000001" },
            "tutor-1",
            CancellationToken.None));

        var revoked = CreatePendingConnection(pregnancy.Id, "vet-1", "tutor-1", "REVOKED00001");
        revoked.Revoke();
        repository.Connection = revoked;
        await Assert.ThrowsAsync<VetInvitationConflictException>(() => service.AcceptInvitationAsync(
            new AcceptVetInvitationRequest { InvitationCode = "REVOKED00001" },
            "tutor-1",
            CancellationToken.None));
    }

    [Fact]
    public async Task Allows_revoke_by_participants_only()
    {
        var pregnancy = CreatePregnancy();
        var repository = new FakeRepository(pregnancy, "tutor-1");
        repository.Connection = CreatePendingConnection(pregnancy.Id, "vet-1", "tutor-1", "INVITE000001");
        var service = new VetConnectionService(repository);

        var revoked = await service.RevokeAsync(repository.Connection.Id, "vet-1", CancellationToken.None);
        Assert.Equal(VetConnectionStatus.Revoked.Value, revoked.Status);

        repository.Connection = CreatePendingConnection(pregnancy.Id, "vet-1", "tutor-1", "INVITE000002");
        await Assert.ThrowsAsync<VetConnectionForbiddenException>(() => service.RevokeAsync(
            repository.Connection.Id, "other-user", CancellationToken.None));
    }

    private static Pregnancy CreatePregnancy()
    {
        return new Pregnancy(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.Date.AddDays(50),
            DateTime.UtcNow.Date.AddDays(-10));
    }

    private static VetConnection CreatePendingConnection(Guid pregnancyId, string vetId, string tutorId, string code)
    {
        return new VetConnection(
            Guid.NewGuid(),
            pregnancyId,
            vetId,
            tutorId,
            code,
            DateTime.UtcNow.AddDays(1));
    }

    private sealed class FakeRepository : IVetConnectionRepository
    {
        private readonly Pregnancy? _pregnancy;
        private readonly string _ownerId;

        public FakeRepository(Pregnancy? pregnancy, string ownerId)
        {
            _pregnancy = pregnancy;
            _ownerId = ownerId;
        }

        public VetConnection? Connection { get; set; }

        public string? LastOwnerId { get; private set; }

        public Task<bool> InvitationCodeExistsAsync(string invitationCode, CancellationToken cancellationToken) =>
            Task.FromResult(Connection is not null && StringComparer.Ordinal.Equals(Connection.InvitationCode, invitationCode));

        public Task AddAsync(VetConnection connection, string ownerId, CancellationToken cancellationToken)
        {
            Connection = connection;
            LastOwnerId = ownerId;
            return Task.CompletedTask;
        }

        public Task<Pregnancy?> GetOwnedPregnancyAsync(Guid petId, Guid pregnancyId, string ownerId, CancellationToken cancellationToken)
        {
            var matches = _pregnancy is not null
                && _pregnancy.Id == pregnancyId
                && _pregnancy.PetId == petId
                && StringComparer.Ordinal.Equals(_ownerId, ownerId);
            return Task.FromResult<Pregnancy?>(matches ? _pregnancy : null);
        }

        public Task<VetConnection?> GetByInvitationCodeAsync(string invitationCode, CancellationToken cancellationToken)
        {
            var normalized = invitationCode.ToUpperInvariant();
            var matches = Connection is not null && StringComparer.Ordinal.Equals(Connection.InvitationCode, normalized);
            return Task.FromResult<VetConnection?>(matches ? Connection : null);
        }

        public Task<VetConnection?> GetByIdAsync(Guid connectionId, CancellationToken cancellationToken)
        {
            var matches = Connection is not null && Connection.Id == connectionId;
            return Task.FromResult<VetConnection?>(matches ? Connection : null);
        }

        public Task<IReadOnlyList<VetConnection>> ListByUserAsync(string userId, CancellationToken cancellationToken)
        {
            if (Connection is null)
                return Task.FromResult<IReadOnlyList<VetConnection>>([]);

            var isParticipant = StringComparer.Ordinal.Equals(Connection.VetUserId, userId)
                || StringComparer.Ordinal.Equals(Connection.TutorUserId, userId);
            return Task.FromResult<IReadOnlyList<VetConnection>>(isParticipant ? [Connection] : []);
        }

        public Task<Guid?> GetPetIdByPregnancyIdAsync(Guid pregnancyId, CancellationToken cancellationToken)
        {
            if (_pregnancy is null || _pregnancy.Id != pregnancyId)
                return Task.FromResult<Guid?>(null);

            return Task.FromResult<Guid?>(_pregnancy.PetId);
        }

        public Task<bool> CanUserAccessPregnancyAsync(Guid pregnancyId, string userId, CancellationToken cancellationToken)
        {
            var isOwner = _pregnancy is not null
                && _pregnancy.Id == pregnancyId
                && StringComparer.Ordinal.Equals(_ownerId, userId);
            var isLinkedVet = Connection is not null
                && Connection.PregnancyId == pregnancyId
                && Connection.Status == VetConnectionStatus.Active.Value
                && StringComparer.Ordinal.Equals(Connection.VetUserId, userId);

            return Task.FromResult(isOwner || isLinkedVet);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
