using VetGest.Domain.ValueObjects;

namespace VetGest.Domain.Entities;

/// <summary>
/// Phase entity. Reference data describing pregnancy phases.
/// Specifies gestational age ranges, fetal development, and maternal changes for each phase.
/// This is shared reference data, NOT owned by a specific user (no OwnerId).
/// </summary>
public class Phase
{
    /// <summary>
    /// Unique identifier (GUID).
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Phase name: Initial, Intermediate, Growth, Final, Birth.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Species this phase applies to: Dog or Cat.
    /// Phases are species-specific because gestation periods differ.
    /// </summary>
    public string Species { get; private set; } = string.Empty;

    /// <summary>
    /// Start day of gestation for this phase (inclusive).
    /// </summary>
    public int StartDayGestation { get; private set; }

    /// <summary>
    /// End day of gestation for this phase (inclusive).
    /// Must be greater than StartDayGestation.
    /// </summary>
    public int EndDayGestation { get; private set; }

    /// <summary>
    /// Phase description (brief summary).
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Fetal development content (Markdown-formatted).
    /// Describes what's happening to the fetuses during this phase.
    /// </summary>
    public string FetalDevelopmentContent { get; private set; } = string.Empty;

    /// <summary>
    /// Maternal changes content (Markdown-formatted).
    /// Describes expected physical and behavioral changes in the mother.
    /// </summary>
    public string MaternalChangesContent { get; private set; } = string.Empty;

    /// <summary>
    /// Shadow property: CreatedAt (audit trail, reference data).
    /// </summary>

    /// <summary>
    /// Creates a new Phase instance.
    /// </summary>
    /// <param name="id">Phase unique identifier.</param>
    /// <param name="name">Phase name (Initial, Intermediate, Growth, Final, Birth).</param>
    /// <param name="species">Species (Dog or Cat).</param>
    /// <param name="startDayGestation">Start day of gestation (inclusive).</param>
    /// <param name="endDayGestation">End day of gestation (inclusive).</param>
    /// <param name="description">Phase description.</param>
    /// <param name="fetalDevelopmentContent">Fetal development details.</param>
    /// <param name="maternalChangesContent">Maternal changes details.</param>
    public Phase(
        Guid id,
        string name,
        string species,
        int startDayGestation,
        int endDayGestation,
        string description,
        string fetalDevelopmentContent,
        string maternalChangesContent)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Phase ID cannot be empty.", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Phase name is required.", nameof(name));

        // Validate name via value object
        _ = ValueObjects.PregnancyPhase.FromString(name);

        if (string.IsNullOrWhiteSpace(species))
            throw new ArgumentException("Species is required.", nameof(species));

        // Validate species via value object
        _ = ValueObjects.Species.FromString(species);

        if (startDayGestation < 0)
            throw new ArgumentException("Start day gestation cannot be negative.", nameof(startDayGestation));

        if (endDayGestation <= startDayGestation)
            throw new ArgumentException(
                "End day gestation must be greater than start day gestation.",
                nameof(endDayGestation));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        if (string.IsNullOrWhiteSpace(fetalDevelopmentContent))
            throw new ArgumentException("Fetal development content is required.", nameof(fetalDevelopmentContent));

        if (string.IsNullOrWhiteSpace(maternalChangesContent))
            throw new ArgumentException("Maternal changes content is required.", nameof(maternalChangesContent));

        Id = id;
        Name = name;
        Species = species;
        StartDayGestation = startDayGestation;
        EndDayGestation = endDayGestation;
        Description = description;
        FetalDevelopmentContent = fetalDevelopmentContent;
        MaternalChangesContent = maternalChangesContent;
    }

    /// <summary>
    /// Determines if a given gestational day falls within this phase.
    /// </summary>
    public bool ContainsDay(int gestationalDay)
    {
        return gestationalDay >= StartDayGestation && gestationalDay <= EndDayGestation;
    }

    /// <summary>
    /// Checks for non-overlapping phases (used for validation).
    /// </summary>
    public bool OverlapsWith(Phase other)
    {
        // Same species only
        if (other.Species != Species)
            return false;

        // Check if ranges overlap
        return !(EndDayGestation < other.StartDayGestation || StartDayGestation > other.EndDayGestation);
    }
}
