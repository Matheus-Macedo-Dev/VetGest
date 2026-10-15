using Microsoft.AspNetCore.Http;
using VetGest.API.Admin;
using VetGest.Contracts.Admin;
using VetGest.Contracts.Common;

namespace VetGest.API.Tests.Infrastructure;

public sealed class TestAdminTutorService : IAdminTutorService
{
    public Task<AdminOperationResult<IReadOnlyList<AdminTutorDto>>> ListTutorsAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new AdminOperationResult<IReadOnlyList<AdminTutorDto>>(
            StatusCodes.Status200OK, ApiResponse<IReadOnlyList<AdminTutorDto>>.Ok([])));

    public Task<AdminOperationResult<AdminTutorDto>> SetActiveAsync(string tutorUserId, bool isActive, CancellationToken cancellationToken) =>
        Task.FromResult(new AdminOperationResult<AdminTutorDto>(
            StatusCodes.Status200OK, ApiResponse<AdminTutorDto>.Ok(new AdminTutorDto { UserId = tutorUserId, IsActive = isActive })));
}
