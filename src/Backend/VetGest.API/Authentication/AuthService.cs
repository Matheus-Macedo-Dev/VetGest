using Microsoft.AspNetCore.Identity;
using VetGest.Contracts.Authentication;
using VetGest.Contracts.Common;
using VetGest.Infrastructure.Identity;

namespace VetGest.API.Authentication;

public sealed record AuthOperationResult(int StatusCode, ApiResponse<AuthResponse> Response);

public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    RoleManager<IdentityRole<string>> roleManager,
    ITokenService tokenService,
    IConfiguration configuration,
    RegistrationValidator registrationValidator)
{
    public async Task<AuthOperationResult> RegisterAsync(RegisterRequest request)
    {
        var validationErrors = registrationValidator.Validate(request);
        if (validationErrors.Count > 0)
            return ValidationResult(validationErrors);

        var email = request.Email.Trim();
        if (await userManager.FindByEmailAsync(email) is not null)
            return new(StatusCodes.Status409Conflict,
                ApiResponse<AuthResponse>.Fail("Já existe uma conta cadastrada com este e-mail."));

        if (!await roleManager.RoleExistsAsync("Tutor"))
            return new(StatusCodes.Status500InternalServerError,
                ApiResponse<AuthResponse>.Fail("O cadastro está temporariamente indisponível. Tente novamente em instantes."));

        var user = new ApplicationUser(email)
        {
            FullName = request.FullName.Trim(),
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors
                .GroupBy(error => error.Code.StartsWith("Password", StringComparison.OrdinalIgnoreCase)
                    ? nameof(RegisterRequest.Password)
                    : nameof(RegisterRequest.Email))
                .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray());

            return ValidationResult(errors);
        }

        var roleResult = await userManager.AddToRoleAsync(user, "Tutor");
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return new(StatusCodes.Status500InternalServerError,
                ApiResponse<AuthResponse>.Fail("Não foi possível concluir o cadastro. Tente novamente."));
        }

        return new(StatusCodes.Status201Created,
            ApiResponse<AuthResponse>.Ok(await CreateAuthResponseAsync(user)));
    }

    public async Task<AuthOperationResult> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return InvalidCredentials();

        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActive)
            return InvalidCredentials();

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        return !signInResult.Succeeded
            ? InvalidCredentials()
            : new(StatusCodes.Status200OK, ApiResponse<AuthResponse>.Ok(await CreateAuthResponseAsync(user)));
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var expiresIn = configuration.GetValue("Jwt:ExpiresInMinutes", 60);
        return new AuthResponse
        {
            AccessToken = tokenService.GenerateToken(user.Id, user.Email!, roles, expiresIn),
            ExpiresIn = expiresIn * 60,
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Roles = roles.ToArray()
        };
    }

    private static AuthOperationResult ValidationResult(IReadOnlyDictionary<string, string[]> errors) =>
        new(StatusCodes.Status400BadRequest, ApiResponse<AuthResponse>.ValidationFailed(
            errors.ToDictionary(pair => pair.Key, pair => pair.Value)));

    private static AuthOperationResult InvalidCredentials() =>
        new(StatusCodes.Status401Unauthorized, ApiResponse<AuthResponse>.Fail("E-mail ou senha inválidos."));
}
