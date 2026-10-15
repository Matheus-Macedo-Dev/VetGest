using Microsoft.AspNetCore.Identity;
using VetGest.Contracts.Admin;
using VetGest.Contracts.Common;
using VetGest.Infrastructure.Identity;

namespace VetGest.API.Admin;

public sealed class AdminTutorService(UserManager<ApplicationUser> userManager) : IAdminTutorService
{
    private const string TutorRole = "Tutor";

    public async Task<AdminOperationResult<IReadOnlyList<AdminTutorDto>>> ListTutorsAsync(CancellationToken cancellationToken)
    {
        var tutors = await userManager.GetUsersInRoleAsync(TutorRole);
        IReadOnlyList<AdminTutorDto> data = tutors.OrderBy(tutor => tutor.FullName).Select(AdminTutorMapper.ToDto).ToList();
        return new(StatusCodes.Status200OK, ApiResponse<IReadOnlyList<AdminTutorDto>>.Ok(data));
    }

    public async Task<AdminOperationResult<AdminTutorDto>> SetActiveAsync(
        string tutorUserId, bool isActive, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(tutorUserId);
        if (user is null || !await userManager.IsInRoleAsync(user, TutorRole))
            return new(StatusCodes.Status404NotFound, ApiResponse<AdminTutorDto>.Fail("Tutor não encontrado."));

        user.IsActive = isActive;
        user.ModifiedAt = DateTime.UtcNow;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return new(StatusCodes.Status500InternalServerError,
                ApiResponse<AdminTutorDto>.Fail("Não foi possível atualizar o tutor."));

        return new(StatusCodes.Status200OK, ApiResponse<AdminTutorDto>.Ok(AdminTutorMapper.ToDto(user)));
    }
}
