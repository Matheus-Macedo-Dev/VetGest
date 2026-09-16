namespace VetGest.Application.Content;

public sealed record PhaseCareContent(
    string Species,
    string Phase,
    string FetalDevelopment,
    string MaternalChanges,
    IReadOnlyList<CareGuidanceItem> GuidanceItems,
    string PublicationStatus,
    string ReviewStatus,
    string Source,
    string? Reviewer,
    DateTime? ReviewDate,
    string Disclaimer,
    string UncertaintyStatement);

public sealed record CareGuidanceItem(string Title, string Guidance);

public interface IPhaseCareContentCatalog
{
    PhaseCareContent? Find(string species, string phase);
}

public sealed class PhaseCareContentCatalog : IPhaseCareContentCatalog
{
    private static readonly IReadOnlyDictionary<(string Species, string Phase), PhaseCareContent> Content =
        BuildContent();

    public PhaseCareContent? Find(string species, string phase) =>
        Content.TryGetValue((species, phase), out var content) ? content : null;

    private static IReadOnlyDictionary<(string Species, string Phase), PhaseCareContent> BuildContent()
    {
        var result = new Dictionary<(string Species, string Phase), PhaseCareContent>(StringTupleComparer.Instance);
        AddSpeciesContent(result, "Dog", "cães");
        AddSpeciesContent(result, "Cat", "gatos");
        return result;
    }

    private static void AddSpeciesContent(
        IDictionary<(string Species, string Phase), PhaseCareContent> result,
        string species,
        string speciesLabel)
    {
        var guidance = new[]
        {
            new CareGuidanceItem("Hidratação", "Mantenha acesso a água limpa e observe o consumo habitual."),
            new CareGuidanceItem("Nutrição", "Converse com a equipe veterinária sobre a alimentação adequada para esta fase."),
            new CareGuidanceItem("Rotina", "Prefira uma rotina calma e previsível, respeitando o descanso do animal."),
            new CareGuidanceItem("Ambiente", "Mantenha o ambiente limpo, seguro e confortável para a mãe."),
            new CareGuidanceItem("Observação", "Registre mudanças de apetite, comportamento e outros fatos para conversar com a equipe veterinária.")
        };

        foreach (var phase in new[] { "Initial", "Intermediate", "Growth", "Final", "Birth" })
        {
            result[(species, phase)] = new PhaseCareContent(
                species,
                phase,
                $"Conteúdo educativo de referência sobre o desenvolvimento fetal de {speciesLabel} nesta fase.",
                $"Conteúdo educativo de referência sobre mudanças maternas de {speciesLabel} nesta fase.",
                guidance,
                "Reference",
                "Unreviewed",
                "Catálogo de referência interno; revisão veterinária pendente.",
                null,
                null,
                "Este conteúdo é educativo e não substitui a avaliação de um médico-veterinário. Não use estas informações para diagnosticar, prescrever ou decidir tratamentos.",
                "A fase e as datas são estimativas. O resultado pode variar conforme a data usada no cálculo e as características individuais do animal.");
        }
    }

    private sealed class StringTupleComparer : IEqualityComparer<(string Species, string Phase)>
    {
        public static readonly StringTupleComparer Instance = new();

        public bool Equals((string Species, string Phase) x, (string Species, string Phase) y) =>
            StringComparer.OrdinalIgnoreCase.Equals(x.Species, y.Species) &&
            StringComparer.OrdinalIgnoreCase.Equals(x.Phase, y.Phase);

        public int GetHashCode((string Species, string Phase) value) =>
            HashCode.Combine(
                StringComparer.OrdinalIgnoreCase.GetHashCode(value.Species),
                StringComparer.OrdinalIgnoreCase.GetHashCode(value.Phase));
    }
}