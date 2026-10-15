using VetGest.Contracts.Admin;

namespace VetGest.API.Admin;

public interface IAdminTutorService
{
    Task<AdminOperationResult<IReadOnlyList<AdminTutorDto>>> ListTutorsAsync(CancellationToken cancellationToken);

    Task<AdminOperationResult<AdminTutorDto>> SetActiveAsync(string tutorUserId, bool isActive, CancellationToken cancellationToken);
}
