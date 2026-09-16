using VetGest.Domain.ValueObjects;

namespace VetGest.Domain.Entities;

/// <summary>
/// PregnancyDiaryEntry entity. Records daily observations during pregnancy.
/// Includes weight, appetite, behavior, temperature, symptoms, and photos.
/// </summary>
public class PregnancyDiaryEntry
{
    /// <summary>
    /// Unique identifier (GUID).
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Foreign key to the Pregnancy this entry belongs to.
    /// </summary>
    public Guid PregnancyId { get; private set; }

    /// <summary>
    /// Navigation property to the Pregnancy.
    /// </summary>
    public Pregnancy Pregnancy { get; private set; } = null!;

    /// <summary>
    /// Date of this diary entry. Cannot be in the future.
    /// </summary>
    public DateTime EntryDate { get; private set; }

    /// <summary>
    /// Weight observation on this date (in kg, optional).
    /// </summary>
    public decimal? Weight { get; private set; }

    /// <summary>
    /// Appetite observation: Normal, Decreased, Increased.
    /// </summary>
    public string? Appetite { get; private set; }

    /// <summary>
    /// Behavior observations (free-form text).
    /// </summary>
    public string? Behavior { get; private set; }

    /// <summary>
    /// Temperature reading (in degrees, optional).
    /// </summary>
    public decimal? Temperature { get; private set; }

    /// <summary>
    /// Comma-separated list of symptoms observed (e.g., "nesting,decreased_appetite,nest_building").
    /// </summary>
    public string? SymptomsList { get; private set; }

    /// <summary>
    /// Comma-separated list of photo URLs or paths.
    /// </summary>
    public string? PhotoUrls { get; private set; }

    /// <summary>
    /// General notes for this diary entry.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Shadow property: OwnerId (inherited from Pet, set via global query filter).
    /// </summary>

    /// <summary>
    /// Shadow property: CreatedAt (audit trail).
    /// </summary>

    /// <summary>
    /// Shadow property: ModifiedAt (audit trail).
    /// </summary>

    /// <summary>
    /// Creates a new PregnancyDiaryEntry instance.
    /// </summary>
    /// <param name="id">Entry unique identifier.</param>
    /// <param name="pregnancyId">ID of the pregnancy.</param>
    /// <param name="entryDate">Date of entry (cannot be in future).</param>
    /// <param name="weight">Weight in kg (optional).</param>
    /// <param name="appetite">Appetite observation (optional).</param>
    /// <param name="behavior">Behavior notes (optional).</param>
    /// <param name="temperature">Temperature reading (optional).</param>
    /// <param name="symptomsList">Comma-separated symptoms (optional).</param>
    /// <param name="photoUrls">Comma-separated photo URLs (optional).</param>
    /// <param name="notes">General notes (optional).</param>
    public PregnancyDiaryEntry(
        Guid id,
        Guid pregnancyId,
        DateTime entryDate,
        decimal? weight = null,
        string? appetite = null,
        string? behavior = null,
        decimal? temperature = null,
        string? symptomsList = null,
        string? photoUrls = null,
        string? notes = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Diary entry ID cannot be empty.", nameof(id));

        if (pregnancyId == Guid.Empty)
            throw new ArgumentException("Pregnancy ID cannot be empty.", nameof(pregnancyId));

        if (entryDate.Kind != DateTimeKind.Utc)
            throw new ArgumentException("EntryDate must be in UTC.", nameof(entryDate));

        if (entryDate.Date > DateTime.UtcNow.Date)
            throw new ArgumentException("EntryDate cannot be in the future.", nameof(entryDate));

        if (weight.HasValue && weight <= 0)
            throw new ArgumentException("Weight must be greater than zero.", nameof(weight));

        if (temperature.HasValue && (temperature <= 0 || temperature > 50))
            throw new ArgumentException("Temperature must be between 0 and 50 degrees.", nameof(temperature));

        // Validate appetite if provided
        if (!string.IsNullOrWhiteSpace(appetite))
        {
            _ = ValueObjects.Appetite.FromString(appetite); // Will throw if invalid
        }

        Id = id;
        PregnancyId = pregnancyId;
        EntryDate = entryDate;
        Weight = weight;
        Appetite = appetite;
        Behavior = behavior;
        Temperature = temperature;
        SymptomsList = symptomsList;
        PhotoUrls = photoUrls;
        Notes = notes;
    }

    /// <summary>
    /// Determines if this entry has abnormal signs (high temp, decreased appetite, unusual behavior).
    /// </summary>
    public bool HasAbnormalSigns()
    {
        var hasHighTemperature = Temperature.HasValue && Temperature > 39.5m; // Normal dog/cat temp ~38-39°C
        var hasDecreatedAppetite = Appetite == ValueObjects.Appetite.Decreased.Value;
        var hasSuspiciousSymptoms = !string.IsNullOrWhiteSpace(SymptomsList)
            && (SymptomsList.Contains("discharge", StringComparison.OrdinalIgnoreCase)
                || SymptomsList.Contains("bleeding", StringComparison.OrdinalIgnoreCase)
                || SymptomsList.Contains("lethargy", StringComparison.OrdinalIgnoreCase));

        return hasHighTemperature || hasDecreatedAppetite || hasSuspiciousSymptoms;
    }

    /// <summary>
    /// Gets the temperature status (Normal, Low, High).
    /// </summary>
    public string? GetTemperatureStatus()
    {
        if (!Temperature.HasValue)
            return null;

        return Temperature switch
        {
            < 37.5m => "Low",
            < 39.5m => "Normal",
            _ => "High"
        };
    }

    /// <summary>
    /// Adds a symptom to the symptoms list.
    /// </summary>
    public void AddSymptom(string symptom)
    {
        if (string.IsNullOrWhiteSpace(symptom))
            return;

        if (string.IsNullOrWhiteSpace(SymptomsList))
        {
            SymptomsList = symptom.ToLowerInvariant();
        }
        else
        {
            var symptoms = SymptomsList.Split(',', StringSplitOptions.TrimEntries);
            if (!symptoms.Contains(symptom.ToLowerInvariant()))
            {
                SymptomsList = $"{SymptomsList},{symptom.ToLowerInvariant()}";
            }
        }
    }

    /// <summary>
    /// Adds a photo URL to the photos list.
    /// </summary>
    public void AddPhoto(string photoUrl)
    {
        if (string.IsNullOrWhiteSpace(photoUrl))
            return;

        if (string.IsNullOrWhiteSpace(PhotoUrls))
        {
            PhotoUrls = photoUrl;
        }
        else
        {
            PhotoUrls = $"{PhotoUrls},{photoUrl}";
        }
    }
}
