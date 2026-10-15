using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetGest.Contracts.Admin;
using VetGest.Contracts.Common;
using VetGest.Domain.ValueObjects;
using VetGest.Infrastructure.Identity;

namespace VetGest.API.Admin;

public sealed class AdminVetService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<string>> roleManager) : IAdminVetService
{
    private const string VetRole = "Vet";

    public async Task<AdminOperationResult<CreatedVetDto>> CreateVetAsync(
        CreateVetRequest request, string adminUserId, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Trim().Length is < 2 or > 256)
            errors[nameof(CreateVetRequest.FullName)] = ["Informe o nome completo, com 2 a 256 caracteres."];

        if (string.IsNullOrWhiteSpace(request.Email) || !new EmailAddressAttribute().IsValid(request.Email))
            errors[nameof(CreateVetRequest.Email)] = ["Informe um e-mail válido."];

        if (!Crmv.TryCreate(request.CrmvNumber, request.CrmvState, out var crmv, out var crmvError))
            errors[nameof(CreateVetRequest.CrmvNumber)] = [crmvError!];

        if (errors.Count > 0)
            return new(StatusCodes.Status400BadRequest, ApiResponse<CreatedVetDto>.ValidationFailed(errors));

        var email = request.Email.Trim();
        if (await userManager.FindByEmailAsync(email) is not null)
            return new(StatusCodes.Status409Conflict,
                ApiResponse<CreatedVetDto>.Fail("Já existe uma conta cadastrada com este e-mail."));

        if (await userManager.Users.AnyAsync(
                user => user.CrmvState == crmv!.State && user.CrmvNumber == crmv.Number, cancellationToken))
            return CrmvConflict();

        if (!await roleManager.RoleExistsAsync(VetRole))
            return new(StatusCodes.Status500InternalServerError,
                ApiResponse<CreatedVetDto>.Fail("O perfil de veterinário não está disponível."));

        var user = new ApplicationUser(email)
        {
            FullName = request.FullName.Trim(),
            EmailConfirmed = true,
            CrmvNumber = crmv!.Number,
            CrmvState = crmv.State,
            CrmvCheckedAt = DateTime.UtcNow,
            CrmvCheckedByUserId = adminUserId
        };

        try
        {
            var created = await userManager.CreateAsync(user);
            if (!created.Succeeded)
                return new(StatusCodes.Status400BadRequest, ApiResponse<CreatedVetDto>.ValidationFailed(
                    new Dictionary<string, string[]>
                    {
                        [nameof(CreateVetRequest.Email)] = created.Errors.Select(error => error.Description).ToArray()
                    }));
        }
        catch (DbUpdateException)
        {
            return CrmvConflict();
        }

        if (!(await userManager.AddToRoleAsync(user, VetRole)).Succeeded)
        {
            await userManager.DeleteAsync(user);
            return new(StatusCodes.Status500InternalServerError,
                ApiResponse<CreatedVetDto>.Fail("Não foi possível concluir o cadastro. Tente novamente."));
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var data = new CreatedVetDto { Vet = ToDto(user), Setup = ToSetup(user, token) };
        return new(StatusCodes.Status201Created, ApiResponse<CreatedVetDto>.Ok(data));
    }

    public async Task<AdminOperationResult<IReadOnlyList<AdminVetDto>>> ListVetsAsync(CancellationToken cancellationToken)
    {
        var vets = await userManager.GetUsersInRoleAsync(VetRole);
        IReadOnlyList<AdminVetDto> data = vets.OrderBy(vet => vet.FullName).Select(ToDto).ToList();
        return new(StatusCodes.Status200OK, ApiResponse<IReadOnlyList<AdminVetDto>>.Ok(data));
    }

    public async Task<AdminOperationResult<VetSetupLinkDto>> IssueSetupTokenAsync(
        string vetUserId, CancellationToken cancellationToken)
    {
        var vet = await FindVetAsync(vetUserId);
        if (vet is null)
            return new(StatusCodes.Status404NotFound, ApiResponse<VetSetupLinkDto>.Fail("Veterinário não encontrado."));

        var token = await userManager.GeneratePasswordResetTokenAsync(vet);
        return new(StatusCodes.Status200OK, ApiResponse<VetSetupLinkDto>.Ok(ToSetup(vet, token)));
    }

    public async Task<AdminOperationResult<AdminVetDto>> SetActiveAsync(
        string vetUserId, bool isActive, CancellationToken cancellationToken)
    {
        var vet = await FindVetAsync(vetUserId);
        if (vet is null)
            return new(StatusCodes.Status404NotFound, ApiResponse<AdminVetDto>.Fail("Veterinário não encontrado."));

        vet.IsActive = isActive;
        vet.ModifiedAt = DateTime.UtcNow;
        var result = await userManager.UpdateAsync(vet);
        if (!result.Succeeded)
            return new(StatusCodes.Status500InternalServerError,
                ApiResponse<AdminVetDto>.Fail("Não foi possível atualizar o veterinário."));

        return new(StatusCodes.Status200OK, ApiResponse<AdminVetDto>.Ok(ToDto(vet)));
    }

    private async Task<ApplicationUser?> FindVetAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user is not null && await userManager.IsInRoleAsync(user, VetRole) ? user : null;
    }

    private static AdminOperationResult<CreatedVetDto> CrmvConflict() =>
        new(StatusCodes.Status409Conflict,
            ApiResponse<CreatedVetDto>.Fail("Já existe um veterinário cadastrado com este CRMV."));

    private static VetSetupLinkDto ToSetup(ApplicationUser user, string token) =>
        new() { UserId = user.Id, Email = user.Email!, Token = token };

    private static AdminVetDto ToDto(ApplicationUser user) => new()
    {
        UserId = user.Id,
        FullName = user.FullName ?? string.Empty,
        Email = user.Email ?? string.Empty,
        Crmv = $"CRMV-{user.CrmvState} {user.CrmvNumber}",
        CrmvNumber = user.CrmvNumber ?? string.Empty,
        CrmvState = user.CrmvState ?? string.Empty,
        IsActive = user.IsActive,
        PasswordSet = user.PasswordHash is not null,
        CreatedAt = user.CreatedAt,
        CrmvCheckedAt = user.CrmvCheckedAt
    };
}
