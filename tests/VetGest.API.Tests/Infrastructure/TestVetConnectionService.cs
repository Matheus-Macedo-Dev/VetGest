using VetGest.Application.Pregnancies;
using VetGest.Contracts.Pregnancies;

namespace VetGest.API.Tests.Infrastructure;

public sealed class TestVetConnectionService : IVetConnectionService
{
    public HashSet<(Guid PregnancyId, string UserId)> AllowedPregnancyAccess { get; } = new();

    public Func<CreateVetInvitationRequest, string, VetConnectionDto>? OnCreateInvitation { get; set; }
    public Func<AcceptVetInvitationRequest, string, VetConnectionDto>? OnAcceptInvitation { get; set; }
    public Func<Guid, string, VetConnectionDto>? OnRevoke { get; set; }
    public Func<string, IReadOnlyList<VetConnectionDto>>? OnList { get; set; }

    public Task<VetConnectionDto> CreateInvitationAsync(
        CreateVetInvitationRequest request,
        string vetUserId,
        CancellationToken cancellationToken)
    {
        if (OnCreateInvitation is not null)
            return Task.FromResult(OnCreateInvitation(request, vetUserId));

        return Task.FromResult(new VetConnectionDto
        {
            Id = Guid.NewGuid(),
            PetId = request.PetId,
            PregnancyId = request.PregnancyId,
            VetUserId = vetUserId,
            TutorUserId = request.TutorUserId,
            InvitationCode = "INVITE123456",
            InvitationExpiresAt = DateTime.UtcNow.AddDays(7),
            Status = "Pending"
        });
    }

    public Task<VetConnectionDto> AcceptInvitationAsync(
        AcceptVetInvitationRequest request,
        string tutorUserId,
        CancellationToken cancellationToken)
    {
        if (OnAcceptInvitation is not null)
            return Task.FromResult(OnAcceptInvitation(request, tutorUserId));

        return Task.FromResult(new VetConnectionDto
        {
            Id = Guid.NewGuid(),
            PetId = Guid.NewGuid(),
            PregnancyId = Guid.NewGuid(),
            VetUserId = "vet-1",
            TutorUserId = tutorUserId,
            InvitationCode = request.InvitationCode,
            InvitationExpiresAt = DateTime.UtcNow.AddDays(7),
            AcceptedAt = DateTime.UtcNow,
            Status = "Active"
        });
    }

    public Task<VetConnectionDto> RevokeAsync(Guid connectionId, string userId, CancellationToken cancellationToken)
    {
        if (OnRevoke is not null)
            return Task.FromResult(OnRevoke(connectionId, userId));

        return Task.FromResult(new VetConnectionDto
        {
            Id = connectionId,
            PetId = Guid.NewGuid(),
            PregnancyId = Guid.NewGuid(),
            VetUserId = "vet-1",
            TutorUserId = "tutor-1",
            InvitationCode = "INVITE123456",
            InvitationExpiresAt = DateTime.UtcNow.AddDays(7),
            Status = "Revoked"
        });
    }

    public Task<IReadOnlyList<VetConnectionDto>> ListForUserAsync(string userId, CancellationToken cancellationToken)
    {
        if (OnList is not null)
            return Task.FromResult(OnList(userId));

        return Task.FromResult<IReadOnlyList<VetConnectionDto>>([]);
    }

    public Task<bool> CanAccessPregnancyAsync(Guid pregnancyId, string userId, CancellationToken cancellationToken)
    {
        return Task.FromResult(AllowedPregnancyAccess.Contains((pregnancyId, userId)));
    }
}
