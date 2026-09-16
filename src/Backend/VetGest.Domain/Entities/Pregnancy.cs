using VetGest.Domain.ValueObjects;

namespace VetGest.Domain.Entities;

/// <summary>
/// Pregnancy entity. Tracks a single pregnancy for a pet.
/// Owns all associated diary entries and vet connections.
/// Includes gestational age calculation and phase determination logic.
/// </summary>
public class Pregnancy
{
    /// <summary>
    /// Unique identifier (GUID).
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Foreign key to the Pet this pregnancy belongs to.
    /// </summary>
    public Guid PetId { get; private set; }

    /// <summary>
    /// Navigation property to the Pet.
    /// </summary>
    public Pet Pet { get; private set; } = null!;

    /// <summary>
    /// Date of mating (optional). Used to estimate ovulation and due date.
    /// </summary>
    public DateTime? MatingDate { get; private set; }

    /// <summary>
    /// Date of ovulation (optional). More precise than mating date for calculations.
    /// </summary>
    public DateTime? OvulationDate { get; private set; }

    /// <summary>
    /// Date when pregnancy was confirmed (via ultrasound, etc.).
    /// </summary>
    public DateTime? ConfirmedAt { get; private set; }

    /// <summary>
    /// Estimated due date calculated from mating/ovulation dates.
    /// Required and non-null.
    /// </summary>
    public DateTime EstimatedDueDate { get; private set; }

    /// <summary>
    /// Current pregnancy phase ID (nullable if phase not yet determined).
    /// </summary>
    public Guid? CurrentPhaseId { get; private set; }

    /// <summary>
    /// Navigation property to current Phase.
    /// </summary>
    public Phase? CurrentPhase { get; private set; }

    /// <summary>
    /// Pregnancy status: Pending, Active, Completed, Cancelled.
    /// </summary>
    public string Status { get; private set; } = PregnancyStatus.Pending.Value;

    /// <summary>
    /// User notes about the pregnancy.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Collection of diary entries for this pregnancy.
    /// </summary>
    public ICollection<PregnancyDiaryEntry> DiaryEntries { get; private set; } = new List<PregnancyDiaryEntry>();

    /// <summary>
    /// Collection of vet connections for this pregnancy (many-to-many via join).
    /// </summary>
    public ICollection<VetConnection> VetConnections { get; private set; } = new List<VetConnection>();

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
    /// Creates a new Pregnancy instance.
    /// Requires either MatingDate or OvulationDate to be provided.
    /// </summary>
    /// <param name="id">Pregnancy unique identifier.</param>
    /// <param name="petId">ID of the pet.</param>
    /// <param name="estimatedDueDate">Calculated estimated due date.</param>
    /// <param name="matingDate">Date of mating (optional).</param>
    /// <param name="ovulationDate">Date of ovulation (optional).</param>
    /// <param name="confirmedAt">Confirmation date (optional).</param>
    /// <param name="notes">User notes (optional).</param>
    public Pregnancy(
        Guid id,
        Guid petId,
        DateTime estimatedDueDate,
        DateTime? matingDate = null,
        DateTime? ovulationDate = null,
        DateTime? confirmedAt = null,
        string? notes = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Pregnancy ID cannot be empty.", nameof(id));

        if (petId == Guid.Empty)
            throw new ArgumentException("Pet ID cannot be empty.", nameof(petId));

        if (matingDate == null && ovulationDate == null)
            throw new ArgumentException(
                "Either MatingDate or OvulationDate must be provided.",
                nameof(matingDate));

        if (matingDate.HasValue && matingDate.Value.Kind != DateTimeKind.Utc)
            throw new ArgumentException("MatingDate must be in UTC.", nameof(matingDate));

        if (ovulationDate.HasValue && ovulationDate.Value.Kind != DateTimeKind.Utc)
            throw new ArgumentException("OvulationDate must be in UTC.", nameof(ovulationDate));

        if (estimatedDueDate.Kind != DateTimeKind.Utc)
            throw new ArgumentException("EstimatedDueDate must be in UTC.", nameof(estimatedDueDate));

        Id = id;
        PetId = petId;
        MatingDate = matingDate;
        OvulationDate = ovulationDate;
        ConfirmedAt = confirmedAt;
        EstimatedDueDate = estimatedDueDate;
        Notes = notes;
    }

    /// <summary>
    /// Calculates the current gestational age in days from the reference date (mating or ovulation).
    /// </summary>
    /// <param name="asOfDate">Reference date for calculation (defaults to today UTC).</param>
    /// <returns>Gestational age in days, or null if neither mating nor ovulation date is set.</returns>
    public int? CalculateGestationalAgeDays(DateTime? asOfDate = null)
    {
        asOfDate ??= DateTime.UtcNow;

        var referenceDate = OvulationDate ?? MatingDate;
        if (referenceDate == null)
            return null;

        return (int)(asOfDate.Value - referenceDate.Value).TotalDays;
    }

    /// <summary>
    /// Determines the current pregnancy phase based on gestational age.
    /// This is a simple placeholder; Phase lookup should be done via Application layer.
    /// </summary>
    /// <returns>Estimated phase based on gestational age, or null if cannot determine.</returns>
    public PregnancyPhase? DeterminePhase()
    {
        var ageInDays = CalculateGestationalAgeDays();
        if (ageInDays == null)
            return null;

        // Dogs: ~63 days; Cats: ~65 days (approximations)
        return ageInDays switch
        {
            < 15 => PregnancyPhase.Initial,
            < 30 => PregnancyPhase.Intermediate,
            < 45 => PregnancyPhase.Growth,
            < 60 => PregnancyPhase.Final,
            _ => PregnancyPhase.Birth
        };
    }

    /// <summary>
    /// Checks if the pregnancy is overdue (past the estimated due date).
    /// </summary>
    /// <param name="asOfDate">Reference date (defaults to today UTC).</param>
    public bool IsOverdue(DateTime? asOfDate = null)
    {
        asOfDate ??= DateTime.UtcNow;
        return asOfDate > EstimatedDueDate;
    }

    /// <summary>
    /// Checks if the pregnancy can be derived (has required date information).
    /// </summary>
    public bool CanBeDerived()
    {
        return MatingDate.HasValue || OvulationDate.HasValue;
    }

    /// <summary>
    /// Activates the pregnancy (changes status to Active).
    /// </summary>
    public void Activate()
    {
        Status = PregnancyStatus.Active.Value;
    }

    /// <summary>
    /// Completes the pregnancy (changes status to Completed).
    /// </summary>
    public void Complete()
    {
        Status = PregnancyStatus.Completed.Value;
    }

    /// <summary>
    /// Cancels the pregnancy (changes status to Cancelled).
    /// </summary>
    public void Cancel()
    {
        Status = PregnancyStatus.Cancelled.Value;
    }

    /// <summary>
    /// Updates the notes for this pregnancy.
    /// </summary>
    public void UpdateNotes(string? newNotes)
    {
        Notes = newNotes;
    }

    public void SetCurrentPhase(Guid? phaseId)
    {
        CurrentPhaseId = phaseId;
    }
}
