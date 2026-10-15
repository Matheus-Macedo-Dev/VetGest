using VetGest.Contracts.Pregnancies;
using VetGest.Domain.Entities;
using VetGest.Domain.ValueObjects;

namespace VetGest.Application.Pregnancies;

public interface IVetPatientService
{
    Task<IReadOnlyList<VetPatientDto>> ListAsync(string vetUserId, CancellationToken cancellationToken);

    /// <summary>Throws <see cref="VetConnectionNotFoundException"/> unless the connection is active and belongs to this vet.</summary>
    Task<VetPatientDetailDto> GetDetailAsync(Guid connectionId, string vetUserId, CancellationToken cancellationToken);
}

public sealed class VetPatientService : IVetPatientService
{
    public const string ReadOnlyNotice =
        "Registros feitos pelo tutor, somente leitura. Não substituem avaliação clínica, e a data prevista do parto é uma estimativa.";

    private readonly IVetConnectionRepository _repository;
    private readonly IPregnancyDiaryService _diaryService;
    private readonly IUserDirectory _userDirectory;

    public VetPatientService(
        IVetConnectionRepository repository,
        IPregnancyDiaryService diaryService,
        IUserDirectory userDirectory)
    {
        _repository = repository;
        _diaryService = diaryService;
        _userDirectory = userDirectory;
    }

    public async Task<IReadOnlyList<VetPatientDto>> ListAsync(string vetUserId, CancellationToken cancellationToken)
    {
        EnsureUserId(vetUserId);
        var connections = (await _repository.ListByUserAsync(vetUserId.Trim(), cancellationToken))
            .Where(connection => IsActiveFor(connection, vetUserId.Trim()))
            .ToArray();

        var names = await GetNamesAsync(connections, cancellationToken);
        return connections.Select(connection => Map(connection, names)).ToArray();
    }

    public async Task<VetPatientDetailDto> GetDetailAsync(Guid connectionId, string vetUserId, CancellationToken cancellationToken)
    {
        EnsureUserId(vetUserId);
        var connection = await _repository.GetByIdAsync(connectionId, cancellationToken);
        if (connection is null || !IsActiveFor(connection, vetUserId.Trim()))
            throw new VetConnectionNotFoundException();

        var names = await GetNamesAsync([connection], cancellationToken);
        var patient = Map(connection, names);

        // The diary service enforces that the tutor still owns an active pet and pregnancy.
        var diary = await _diaryService.ListAsync(
            patient.PetId, patient.PregnancyId, connection.TutorUserId!, cancellationToken);

        return new VetPatientDetailDto
        {
            Patient = patient,
            Diary = diary ?? Array.Empty<Contracts.Pregnancies.DiaryEntryDto>(),
            Notice = ReadOnlyNotice
        };
    }

    private static bool IsActiveFor(VetConnection connection, string vetUserId) =>
        StringComparer.Ordinal.Equals(connection.VetUserId, vetUserId)
        && connection.Status == VetConnectionStatus.Active.Value
        && connection.PregnancyId is not null
        && connection.TutorUserId is not null
        && connection.Pregnancy?.Pet is not null;

    private async Task<IReadOnlyDictionary<string, string>> GetNamesAsync(
        IEnumerable<VetConnection> connections, CancellationToken cancellationToken) =>
        await _userDirectory.GetDisplayNamesAsync(
            connections.Select(connection => connection.TutorUserId!).Distinct().ToArray(), cancellationToken);

    private static VetPatientDto Map(VetConnection connection, IReadOnlyDictionary<string, string> names)
    {
        var pregnancy = connection.Pregnancy!;
        return new VetPatientDto
        {
            ConnectionId = connection.Id,
            PetId = pregnancy.PetId,
            PetName = pregnancy.Pet.Name,
            Species = pregnancy.Pet.Species,
            Breed = pregnancy.Pet.Breed,
            PregnancyId = pregnancy.Id,
            PregnancyStatus = pregnancy.Status,
            EstimatedDueDate = pregnancy.EstimatedDueDate,
            TutorName = names.GetValueOrDefault(connection.TutorUserId!) ?? string.Empty,
            ConnectedAt = connection.AcceptedAt
        };
    }

    private static void EnsureUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("É necessário ter um usuário autenticado.", nameof(userId));
    }
}
