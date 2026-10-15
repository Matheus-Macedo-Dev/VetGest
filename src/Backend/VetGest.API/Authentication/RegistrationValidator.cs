using System.ComponentModel.DataAnnotations;
using VetGest.Contracts.Authentication;

namespace VetGest.API.Authentication;

public sealed class RegistrationValidator
{
    public IReadOnlyDictionary<string, string[]> Validate(RegisterRequest request, string password, string confirmPassword)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Trim().Length is < 2 or > 256)
            errors[nameof(RegisterRequest.FullName)] = ["Informe seu nome completo, com 2 a 256 caracteres."];

        if (string.IsNullOrWhiteSpace(request.Email) || !new EmailAddressAttribute().IsValid(request.Email))
            errors[nameof(RegisterRequest.Email)] = ["Informe um e-mail válido."];

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            errors[nameof(RegisterRequest.Password)] = ["A senha deve ter pelo menos 8 caracteres."];
        else if (password.Length > PasswordTransport.MaxPasswordLength)
            errors[nameof(RegisterRequest.Password)] =
                [$"A senha deve ter no máximo {PasswordTransport.MaxPasswordLength} caracteres."];

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            errors[nameof(RegisterRequest.ConfirmPassword)] = ["As senhas informadas não conferem."];

        return errors;
    }
}
