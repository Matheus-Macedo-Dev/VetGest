using VetGest.Contracts.Admin;
using VetGest.Contracts.Common;

namespace VetGest.API.Admin;

public sealed record AdminOperationResult<T>(int StatusCode, ApiResponse<T> Response);

public interface IAdminVetService
{
    Task<AdminOperationResult<CreatedVetDto>> CreateVetAsync(CreateVetRequest request, string adminUserId, CancellationToken cancellationToken);

    Task<AdminOperationResult<IReadOnlyList<AdminVetDto>>> ListVetsAsync(CancellationToken cancellationToken);

    Task<AdminOperationResult<VetSetupLinkDto>> IssueSetupTokenAsync(string vetUserId, CancellationToken cancellationToken);

    Task<AdminOperationResult<AdminVetDto>> SetActiveAsync(string vetUserId, bool isActive, CancellationToken cancellationToken);
}
