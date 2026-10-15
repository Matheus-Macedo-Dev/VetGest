using VetGest.Contracts.Pregnancies;

namespace VetGest.Application.Pregnancies;

public interface IVetConnectionService
{
    Task<VetConnectionDto> CreateInvitationAsync(
        CreateVetInvitationRequest request,
        string vetUserId,
        CancellationToken cancellationToken);

    Task<VetConnectionDto> CreateCodeAsync(
        string vetUserId,
        CancellationToken cancellationToken);

    Task<VetConnectionDto> AcceptInvitationAsync(
        AcceptVetInvitationRequest request,
        string tutorUserId,
        CancellationToken cancellationToken);

    Task<VetConnectionDto> RevokeAsync(
        Guid connectionId,
        string userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<VetConnectionDto>> ListForUserAsync(
        string userId,
        CancellationToken cancellationToken);

    /// <summary>True only for a participant of an Active connection.</summary>
    Task<bool> CanChatAsync(Guid connectionId, string userId, CancellationToken cancellationToken);

    Task<bool> CanAccessPregnancyAsync(
        Guid pregnancyId,
        string userId,
        CancellationToken cancellationToken);
}
