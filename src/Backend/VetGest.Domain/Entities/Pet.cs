using VetGest.Domain.ValueObjects;

namespace VetGest.Domain.Entities;

/// <summary>
/// Pet aggregate root. Represents a pet under veterinary pregnancy tracking.
/// Owns all associated pregnancies and diary entries via the OwnerId relationship.
/// This is the primary entity for multi-tenant data isolation.
/// </summary>
public class Pet
{
    /// <summary>
    /// Unique identifier (GUID).
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Pet name. Required, max 100 characters.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Pet species: Dog or Cat.
    /// </summary>
    public string Species { get; private set; } = string.Empty;

    /// <summary>
    /// Pet breed. Max 100 characters.
    /// </summary>
    public string? Breed { get; private set; }

    /// <summary>
    /// Pet date of birth.
    /// </summary>
    public DateTime? DateOfBirth { get; private set; }

    /// <summary>
    /// Current weight in kilograms (decimal to support precise measurements).
    /// </summary>
    public decimal? CurrentWeight { get; private set; }

    /// <summary>
    /// URL to a photo of the pet.
    /// </summary>
    public string? PhotoUrl { get; private set; }

    /// <summary>
    /// Whether this pet record is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Collection of pregnancies associated with this pet.
    /// </summary>
    public ICollection<Pregnancy> Pregnancies { get; private set; } = new List<Pregnancy>();

    /// <summary>
    /// Shadow property: OwnerId (set via EF Core for global query filter).
    /// </summary>

    /// <summary>
    /// Shadow property: CreatedAt (audit trail).
    /// </summary>

    /// <summary>
    /// Shadow property: ModifiedAt (audit trail).
    /// </summary>

    /// <summary>
    /// Creates a new Pet instance.
    /// </summary>
    /// <param name="id">Pet unique identifier.</param>
    /// <param name="name">Pet name (required, max 100 chars).</param>
    /// <param name="species">Pet species (Dog or Cat).</param>
    /// <param name="breed">Pet breed (optional, max 100 chars).</param>
    /// <param name="dateOfBirth">Pet date of birth (optional).</param>
    /// <param name="currentWeight">Current weight in kg (optional).</param>
    /// <param name="photoUrl">Photo URL (optional).</param>
    public Pet(
        Guid id,
        string name,
        string species,
        string? breed = null,
        DateTime? dateOfBirth = null,
        decimal? currentWeight = null,
        string? photoUrl = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Pet ID cannot be empty.", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Pet name is required.", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Pet name cannot exceed 100 characters.", nameof(name));

        if (string.IsNullOrWhiteSpace(species))
            throw new ArgumentException("Pet species is required.", nameof(species));

        // Validate species via value object
        _ = ValueObjects.Species.FromString(species); // Will throw if invalid

        if (breed != null && breed.Length > 100)
            throw new ArgumentException("Pet breed cannot exceed 100 characters.", nameof(breed));

        if (currentWeight.HasValue && currentWeight <= 0)
            throw new ArgumentException("Pet weight must be greater than zero.", nameof(currentWeight));

        Id = id;
        Name = name;
        Species = species;
        Breed = breed;
        DateOfBirth = dateOfBirth;
        CurrentWeight = currentWeight;
        PhotoUrl = photoUrl;
    }

    /// <summary>
    /// Determines if the pet can have a pregnancy record created or updated.
    /// </summary>
    public bool CanUpdatePregnancy()
    {
        return IsActive;
    }

    /// <summary>
    /// Updates the pet's current weight.
    /// </summary>
    /// <param name="newWeight">New weight in kilograms.</param>
    public void UpdateWeight(decimal newWeight)
    {
        if (newWeight <= 0)
            throw new ArgumentException("Weight must be greater than zero.", nameof(newWeight));

        CurrentWeight = newWeight;
    }

    /// <summary>
    /// Deactivates the pet record.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Reactivates the pet record.
    /// </summary>
    public void Reactivate()
    {
        IsActive = true;
    }
}
