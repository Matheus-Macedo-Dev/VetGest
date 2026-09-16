using VetGest.Contracts.Pregnancies;
using VetGest.Domain.Entities;
using VetGest.Domain.ValueObjects;
using VetGest.Application.Pets;

namespace VetGest.Application.Pregnancies;

public interface IPregnancyDiaryRepository
{
    Task AddAsync(PregnancyDiaryEntry entry, string ownerId, CancellationToken cancellationToken);
    Task<bool> ExistsForDateAsync(Guid pregnancyId, DateTime entryDate, string ownerId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PregnancyDiaryEntry>> ListOwnedAsync(Guid pregnancyId, string ownerId, CancellationToken cancellationToken);
}

public sealed class DiaryValidationException : Exception
{
    public DiaryValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Diary entry validation failed.") => Errors = errors;

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}

public sealed class DiaryDuplicateDateException : Exception;

public interface IPregnancyDiaryService
{
    Task<DiaryEntryDto> CreateAsync(Guid petId, Guid pregnancyId, CreateDiaryEntryRequest request, string ownerId, CancellationToken cancellationToken);
    Task<IReadOnlyList<DiaryEntryDto>?> ListAsync(Guid petId, Guid pregnancyId, string ownerId, CancellationToken cancellationToken);
}

public sealed class PregnancyDiaryService : IPregnancyDiaryService
{
    private readonly IPetRepository _petRepository;
    private readonly IPregnancyRepository _pregnancyRepository;
    private readonly IPregnancyDiaryRepository _diaryRepository;

    public PregnancyDiaryService(IPetRepository petRepository, IPregnancyRepository pregnancyRepository, IPregnancyDiaryRepository diaryRepository)
    {
        _petRepository = petRepository;
        _pregnancyRepository = pregnancyRepository;
        _diaryRepository = diaryRepository;
    }

    public async Task<DiaryEntryDto> CreateAsync(Guid petId, Guid pregnancyId, CreateDiaryEntryRequest request, string ownerId, CancellationToken cancellationToken)
    {
        EnsureOwnerId(ownerId);
        var errors = Validate(request);
        if (errors.Count > 0)
            throw new DiaryValidationException(errors);

        if (await GetActiveOwnedPregnancyAsync(petId, pregnancyId, ownerId, cancellationToken) is null)
            throw new DiaryParentNotFoundException();

        var entryDate = NormalizeDate(request.EntryDate);
        if (await _diaryRepository.ExistsForDateAsync(pregnancyId, entryDate, ownerId, cancellationToken))
            throw new DiaryDuplicateDateException();

        var entry = new PregnancyDiaryEntry(
            Guid.NewGuid(), pregnancyId, entryDate, request.WeightKg,
            request.Appetite?.Trim(), request.Behavior?.Trim(), request.TemperatureCelsius,
            JoinValues(request.Symptoms), JoinValues(request.PhotoReferences), request.Notes?.Trim());

        await _diaryRepository.AddAsync(entry, ownerId, cancellationToken);
        return Map(entry);
    }

    public async Task<IReadOnlyList<DiaryEntryDto>?> ListAsync(Guid petId, Guid pregnancyId, string ownerId, CancellationToken cancellationToken)
    {
        EnsureOwnerId(ownerId);
        if (await GetActiveOwnedPregnancyAsync(petId, pregnancyId, ownerId, cancellationToken) is null)
            return null;

        var entries = await _diaryRepository.ListOwnedAsync(pregnancyId, ownerId, cancellationToken);
        return entries.Select(Map).ToArray();
    }

    private async Task<Pregnancy?> GetActiveOwnedPregnancyAsync(Guid petId, Guid pregnancyId, string ownerId, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetOwnedAsync(petId, ownerId, cancellationToken);
        if (pet is null || !pet.IsActive)
            return null;

        var pregnancy = await _pregnancyRepository.GetOwnedAsync(petId, pregnancyId, ownerId, cancellationToken);
        return pregnancy is null || pregnancy.Status is "Completed" or "Cancelled" ? null : pregnancy;
    }

    private static Dictionary<string, string[]> Validate(CreateDiaryEntryRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        if (request.EntryDate == default || request.EntryDate.Date > DateTime.UtcNow.Date)
            errors[nameof(request.EntryDate)] = new[] { "Informe a data da observação. Ela não pode ser futura." };
        if (request.WeightKg is <= 0 or > 999.99m)
            errors[nameof(request.WeightKg)] = new[] { "O peso deve ser maior que zero e ter no máximo 999,99 kg." };
        if (request.TemperatureCelsius is < 25m or > 45m)
            errors[nameof(request.TemperatureCelsius)] = new[] { "A temperatura deve estar entre 25 e 45 °C." };
        if (!string.IsNullOrWhiteSpace(request.Appetite))
        {
            try { _ = Appetite.FromString(request.Appetite.Trim()); }
            catch (ArgumentException) { errors[nameof(request.Appetite)] = new[] { "O apetite deve ser normal, diminuído ou aumentado." }; }
        }
        if (request.Behavior?.Length > 1000) errors[nameof(request.Behavior)] = new[] { "O comportamento deve ter no máximo 1.000 caracteres." };
        if (request.Notes?.Length > 1000) errors[nameof(request.Notes)] = new[] { "As notas devem ter no máximo 1.000 caracteres." };
        ValidateValues(request.Symptoms, nameof(request.Symptoms), 500, errors);
        ValidateValues(request.PhotoReferences, nameof(request.PhotoReferences), 2000, errors);
        if (!HasObservation(request)) errors["Observation"] = new[] { "Informe pelo menos uma observação além da data." };
        return errors;
    }

    private static void ValidateValues(string[]? values, string name, int totalLimit, Dictionary<string, string[]> errors)
    {
        if (values is null) return;
        var nonEmptyValues = values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToArray();
        if (nonEmptyValues.Any(value => value.Length > 250) || nonEmptyValues.Sum(value => value.Length) + Math.Max(0, nonEmptyValues.Length - 1) > totalLimit)
            errors[name] = new[] { $"{name} contains invalid or oversized values." };
    }

    private static bool HasObservation(CreateDiaryEntryRequest request) =>
        request.WeightKg.HasValue || !string.IsNullOrWhiteSpace(request.Appetite) ||
        !string.IsNullOrWhiteSpace(request.Behavior) || request.TemperatureCelsius.HasValue ||
        request.Symptoms?.Any(value => !string.IsNullOrWhiteSpace(value)) == true ||
        request.PhotoReferences?.Any(value => !string.IsNullOrWhiteSpace(value)) == true ||
        !string.IsNullOrWhiteSpace(request.Notes);

    private static string? JoinValues(string[]? values) => values is null ? null : string.Join(',', values.Select(value => value.Trim()).Where(value => value.Length > 0));
    private static DateTime NormalizeDate(DateTime value) => DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
    private static void EnsureOwnerId(string ownerId) { if (string.IsNullOrWhiteSpace(ownerId)) throw new ArgumentException("É necessário ter um usuário autenticado.", nameof(ownerId)); }

    private static DiaryEntryDto Map(PregnancyDiaryEntry entry) => new()
    {
        Id = entry.Id, PregnancyId = entry.PregnancyId, EntryDate = entry.EntryDate,
        WeightKg = entry.Weight, Appetite = entry.Appetite, Behavior = entry.Behavior,
        TemperatureCelsius = entry.Temperature, Symptoms = SplitValues(entry.SymptomsList),
        PhotoReferences = SplitValues(entry.PhotoUrls), Notes = entry.Notes
    };

    private static string[] SplitValues(string? values) => string.IsNullOrWhiteSpace(values)
        ? Array.Empty<string>() : values.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

public sealed class DiaryParentNotFoundException : Exception;