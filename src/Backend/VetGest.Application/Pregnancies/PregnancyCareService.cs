using VetGest.Application.Content;
using VetGest.Application.Pets;
using VetGest.Contracts.Pregnancies;
using VetGest.Domain.Entities;

namespace VetGest.Application.Pregnancies;

public interface IPregnancyCareService
{
    Task<CareContentDto?> GetAsync(Guid petId, Guid pregnancyId, string ownerId, CancellationToken cancellationToken);
}

public sealed class PregnancyCareService : IPregnancyCareService
{
    private readonly IPetRepository _petRepository;
    private readonly IPregnancyRepository _pregnancyRepository;
    private readonly PregnancyCalculationPolicy _calculationPolicy;
    private readonly IPhaseCareContentCatalog _contentCatalog;

    public PregnancyCareService(
        IPetRepository petRepository,
        IPregnancyRepository pregnancyRepository,
        PregnancyCalculationPolicy calculationPolicy,
        IPhaseCareContentCatalog contentCatalog)
    {
        _petRepository = petRepository;
        _pregnancyRepository = pregnancyRepository;
        _calculationPolicy = calculationPolicy;
        _contentCatalog = contentCatalog;
    }

    public async Task<CareContentDto?> GetAsync(
        Guid petId,
        Guid pregnancyId,
        string ownerId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
            throw new ArgumentException("É necessário ter um usuário autenticado.", nameof(ownerId));

        // Both lookups are required: a pregnancy identifier alone must not establish pet ownership.
        var pet = await _petRepository.GetOwnedAsync(petId, ownerId, cancellationToken);
        if (pet is null || !pet.IsActive)
            return null;

        var pregnancy = await _pregnancyRepository.GetOwnedAsync(petId, pregnancyId, ownerId, cancellationToken);
        if (pregnancy is null)
            return null;

        var calculation = _calculationPolicy.Calculate(
            pet.Species,
            pregnancy.MatingDate,
            pregnancy.OvulationDate,
            DateTime.UtcNow);

        var phase = calculation.GestationalDay >= 0
            ? await _pregnancyRepository.GetPhaseForDayAsync(pet.Species, calculation.GestationalDay, cancellationToken)
            : null;

        if (phase is null)
            return EmptyContent(pet.Species, calculation);

        var content = _contentCatalog.Find(pet.Species, phase.Name);
        return content is null
            ? EmptyContent(pet.Species, calculation, phase)
            : Map(content, phase, calculation);
    }

    private static CareContentDto EmptyContent(
        string species,
        PregnancyCalculationResult calculation,
        Phase? phase = null) => new()
    {
        IsAvailable = false,
        Species = species,
        Phase = phase?.Name,
        PhaseId = phase?.Id,
        StartDayGestation = phase?.StartDayGestation,
        EndDayGestation = phase?.EndDayGestation,
        PublicationStatus = "Unpublished",
        ReviewStatus = "Unreviewed",
        Disclaimer = "Não há conteúdo publicado para esta combinação de espécie e fase. Este conteúdo não substitui a avaliação de um médico-veterinário.",
        UncertaintyStatement = BuildUncertainty(calculation)
    };

    private static CareContentDto Map(
        PhaseCareContent content,
        Phase phase,
        PregnancyCalculationResult calculation) => new()
    {
        IsAvailable = true,
        Species = content.Species,
        Phase = phase.Name,
        PhaseId = phase.Id,
        StartDayGestation = phase.StartDayGestation,
        EndDayGestation = phase.EndDayGestation,
        FetalDevelopment = content.FetalDevelopment,
        MaternalChanges = content.MaternalChanges,
        GuidanceItems = content.GuidanceItems
            .Select(item => new CareGuidanceItemDto { Title = item.Title, Guidance = item.Guidance })
            .ToArray(),
        PublicationStatus = content.PublicationStatus,
        ReviewStatus = content.ReviewStatus,
        Source = content.Source,
        Reviewer = content.Reviewer,
        ReviewDate = content.ReviewDate,
        Disclaimer = content.Disclaimer,
        UncertaintyStatement = BuildUncertainty(calculation, content.UncertaintyStatement)
    };

    private static string BuildUncertainty(PregnancyCalculationResult calculation, string? contentUncertainty = null) =>
        $"{contentUncertainty ?? "A fase e as datas são estimativas."} {calculation.Basis switch
        {
            PregnancyCalculationBasis.OvulationDate => "O cálculo usa a data registrada de ovulação ou progesterona.",
            _ => "O cálculo usa a data principal de cobertura; o momento real pode variar."
        }}";
}