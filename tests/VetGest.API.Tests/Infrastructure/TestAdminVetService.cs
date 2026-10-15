using Microsoft.AspNetCore.Http;
using VetGest.API.Admin;
using VetGest.Contracts.Admin;
using VetGest.Contracts.Common;

namespace VetGest.API.Tests.Infrastructure;

public sealed class TestAdminVetService : IAdminVetService
{
    public Task<AdminOperationResult<CreatedVetDto>> CreateVetAsync(
        CreateVetRequest request, string adminUserId, CancellationToken cancellationToken) =>
        Task.FromResult(new AdminOperationResult<CreatedVetDto>(
            StatusCodes.Status201Created,
            ApiResponse<CreatedVetDto>.Ok(new CreatedVetDto { Vet = new AdminVetDto { Email = request.Email } })));

    public Task<AdminOperationResult<IReadOnlyList<AdminVetDto>>> ListVetsAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new AdminOperationResult<IReadOnlyList<AdminVetDto>>(
            StatusCodes.Status200OK, ApiResponse<IReadOnlyList<AdminVetDto>>.Ok([])));

    public Task<AdminOperationResult<VetSetupLinkDto>> IssueSetupTokenAsync(string vetUserId, CancellationToken cancellationToken) =>
        Task.FromResult(new AdminOperationResult<VetSetupLinkDto>(
            StatusCodes.Status200OK, ApiResponse<VetSetupLinkDto>.Ok(new VetSetupLinkDto { UserId = vetUserId })));

    public Task<AdminOperationResult<AdminVetDto>> SetActiveAsync(string vetUserId, bool isActive, CancellationToken cancellationToken) =>
        Task.FromResult(new AdminOperationResult<AdminVetDto>(
            StatusCodes.Status200OK, ApiResponse<AdminVetDto>.Ok(new AdminVetDto { UserId = vetUserId, IsActive = isActive })));
}
