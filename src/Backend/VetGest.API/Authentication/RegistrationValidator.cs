using System.ComponentModel.DataAnnotations;
using VetGest.Contracts.Authentication;

namespace VetGest.API.Authentication;

public sealed class RegistrationValidator
{
    public IReadOnlyDictionary<string, string[]> Validate(RegisterRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Trim().Length is < 2 or > 256)
            errors[nameof(RegisterRequest.FullName)] = ["Informe seu nome completo, com 2 a 256 caracteres."];

        if (string.IsNullOrWhiteSpace(request.Email) || !new EmailAddressAttribute().IsValid(request.Email))
            errors[nameof(RegisterRequest.Email)] = ["Informe um e-mail válido."];

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            errors[nameof(RegisterRequest.Password)] = ["A senha deve ter pelo menos 8 caracteres."];

        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
            errors[nameof(RegisterRequest.ConfirmPassword)] = ["As senhas informadas não conferem."];

        return errors;
    }
}
