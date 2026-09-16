using VetGest.Contracts.Pets;
using VetGest.Domain.Entities;
using VetGest.Domain.ValueObjects;

namespace VetGest.Application.Pets;

public sealed class PetValidationException : Exception
{
    public PetValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Pet validation failed.") => Errors = errors;

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}

public interface IPetService
{
    Task<PetDto> CreateAsync(CreatePetRequest request, string ownerId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PetDto>> ListAsync(string ownerId, CancellationToken cancellationToken);
    Task<PetDto?> GetAsync(Guid petId, string ownerId, CancellationToken cancellationToken);
}

public sealed class PetService : IPetService
{
    private readonly IPetRepository _repository;

    public PetService(IPetRepository repository) => _repository = repository;

    public async Task<PetDto> CreateAsync(CreatePetRequest request, string ownerId, CancellationToken cancellationToken)
    {
        var errors = Validate(request, ownerId);
        if (errors.Count > 0)
            throw new PetValidationException(errors);

        var pet = new Pet(
            Guid.NewGuid(),
            request.Name.Trim(),
            Species.FromString(request.Species.Trim()).Value,
            request.Breed?.Trim(),
            request.DateOfBirth,
            request.CurrentWeight,
            request.PhotoUrl?.Trim());

        await _repository.AddAsync(pet, ownerId, cancellationToken);
        return Map(pet);
    }

    public async Task<IReadOnlyList<PetDto>> ListAsync(string ownerId, CancellationToken cancellationToken)
    {
        EnsureOwnerId(ownerId);
        var pets = await _repository.ListOwnedAsync(ownerId, cancellationToken);
        return pets.Select(Map).ToArray();
    }

    public async Task<PetDto?> GetAsync(Guid petId, string ownerId, CancellationToken cancellationToken)
    {
        EnsureOwnerId(ownerId);
        var pet = await _repository.GetOwnedAsync(petId, ownerId, cancellationToken);
        return pet is null ? null : Map(pet);
    }

    private static Dictionary<string, string[]> Validate(CreatePetRequest request, string ownerId)
    {
        EnsureOwnerId(ownerId);
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 100)
            errors[nameof(request.Name)] = new[] { "Name is required and must be at most 100 characters." };

        try
        {
            if (string.IsNullOrWhiteSpace(request.Species))
                throw new ArgumentException();

            _ = Species.FromString(request.Species.Trim());
        }
        catch (Exception) { errors[nameof(request.Species)] = new[] { "Species must be Dog or Cat." }; }

        if (request.Breed?.Length > 100)
            errors[nameof(request.Breed)] = new[] { "Breed must be at most 100 characters." };

        if (request.CurrentWeight is <= 0 or > 999.99m)
            errors[nameof(request.CurrentWeight)] = new[] { "O peso deve ser maior que zero e ter no máximo 999,99 kg." };

        if (request.DateOfBirth is { } dateOfBirth && (dateOfBirth.Date > DateTime.UtcNow.Date || dateOfBirth.Year < 1900))
            errors[nameof(request.DateOfBirth)] = new[] { "Date of birth must be between 1900 and today." };

        return errors;
    }

    private static void EnsureOwnerId(string ownerId)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
            throw new ArgumentException("É necessário ter um usuário autenticado.", nameof(ownerId));
    }

    private static PetDto Map(Pet pet) => new()
    {
        Id = pet.Id,
        Name = pet.Name,
        Species = pet.Species,
        Breed = pet.Breed,
        DateOfBirth = pet.DateOfBirth,
        CurrentWeight = pet.CurrentWeight,
        PhotoUrl = pet.PhotoUrl,
        IsActive = pet.IsActive
    };
}