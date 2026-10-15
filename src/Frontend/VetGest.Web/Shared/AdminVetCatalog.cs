using VetGest.Contracts.Admin;

namespace VetGest.Web.Shared;

public static class AdminVetCatalog
{
    public static readonly string[] Ufs =
    [
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE",
        "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    ];

    public static string StatusLabel(AdminVetDto vet) =>
        !vet.IsActive ? "Desativado" : vet.PasswordSet ? "Ativo" : "Aguardando senha";
}
