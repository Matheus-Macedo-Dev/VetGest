using VetGest.Application.Pets;
using VetGest.Contracts.Pregnancies;
using VetGest.Domain.Entities;

namespace VetGest.Application.Pregnancies;

public interface IPregnancyService
{
    Task<PregnancyDto> CreateAsync(Guid petId, CreatePregnancyRequest request, string ownerId, CancellationToken cancellationToken);
    Task<PregnancyDto?> GetCurrentAsync(Guid petId, string ownerId, CancellationToken cancellationToken);
}

public sealed class PregnancyService : IPregnancyService
{
    private readonly IPetRepository _petRepository;
    private readonly IPregnancyRepository _repository;
    private readonly PregnancyCalculationPolicy _policy;

    public PregnancyService(IPetRepository petRepository, IPregnancyRepository repository, PregnancyCalculationPolicy policy)
    {
        _petRepository = petRepository;
        _repository = repository;
        _policy = policy;
    }

    public async Task<PregnancyDto> CreateAsync(Guid petId, CreatePregnancyRequest request, string ownerId, CancellationToken cancellationToken)
    {
        EnsureOwnerId(ownerId);
        var pet = await _petRepository.GetOwnedAsync(petId, ownerId, cancellationToken);
        if (pet is null || !pet.IsActive)
            throw new PregnancyNotFoundException();

        var calculation = _policy.Calculate(pet.Species, request.MatingDate, request.OvulationDate, DateTime.UtcNow);
        var phase = await FindPhaseAsync(pet.Species, calculation.GestationalDay, cancellationToken);
        var pregnancy = new Pregnancy(
            Guid.NewGuid(), pet.Id, calculation.EstimatedDueDate,
            Normalize(request.MatingDate), Normalize(request.OvulationDate),
            Normalize(request.ConfirmedAt), request.Notes?.Trim());

        pregnancy.SetCurrentPhase(phase?.Id);
        await _repository.AddAsync(pregnancy, ownerId, cancellationToken);
        return Map(pregnancy, pet.Species, calculation, phase);
    }

    public async Task<PregnancyDto?> GetCurrentAsync(Guid petId, string ownerId, CancellationToken cancellationToken)
    {
        EnsureOwnerId(ownerId);
        var pet = await _petRepository.GetOwnedAsync(petId, ownerId, cancellationToken);
        if (pet is null || !pet.IsActive)
            return null;

        var pregnancy = await _repository.GetCurrentOwnedAsync(petId, ownerId, cancellationToken);
        if (pregnancy is null)
            return null;

        var calculation = _policy.Calculate(pet.Species, pregnancy.MatingDate, pregnancy.OvulationDate, DateTime.UtcNow);
        var phase = await FindPhaseAsync(pet.Species, calculation.GestationalDay, cancellationToken);
        return Map(pregnancy, pet.Species, calculation, phase);
    }

    private async Task<Phase?> FindPhaseAsync(string species, int gestationalDay, CancellationToken cancellationToken)
    {
        if (gestationalDay < 0)
            return null;

        return await _repository.GetPhaseForDayAsync(species, gestationalDay, cancellationToken)
            ?? await _repository.GetLastPhaseAsync(species, cancellationToken);
    }

    private static PregnancyDto Map(Pregnancy pregnancy, string species, PregnancyCalculationResult calculation, Phase? phase) => new()
    {
        Id = pregnancy.Id,
        PetId = pregnancy.PetId,
        Species = species,
        MatingDate = pregnancy.MatingDate,
        OvulationDate = pregnancy.OvulationDate,
        ConfirmedAt = pregnancy.ConfirmedAt,
        EstimatedDueDate = pregnancy.EstimatedDueDate,
        Status = pregnancy.Status,
        Notes = pregnancy.Notes,
        Calculation = new PregnancyCalculationSummary
        {
            Basis = calculation.Basis,
            ReferenceDate = calculation.ReferenceDate,
            GestationalDay = calculation.GestationalDay,
            EstimatedDueDate = calculation.EstimatedDueDate,
            IsOverdue = calculation.IsOverdue,
            Phase = phase?.Name,
            PhaseId = phase?.Id,
            UncertaintyStatement = calculation.Basis == PregnancyCalculationBasis.OvulationDate
                ? "Esta é uma data estimada com base na data registrada de ovulação ou progesterona."
                : "Esta é uma data estimada com base na data principal de cobertura; o momento real pode variar."
        }
    };

    private static DateTime? Normalize(DateTime? value) => value is null
        ? null : DateTime.SpecifyKind(value.Value.Date, DateTimeKind.Utc);

    private static void EnsureOwnerId(string ownerId)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
            throw new ArgumentException("É necessário ter um usuário autenticado.", nameof(ownerId));
    }
}

public sealed class PregnancyNotFoundException : Exception;