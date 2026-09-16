using VetGest.Contracts.Pregnancies;
using VetGest.Domain.ValueObjects;

namespace VetGest.Application.Pregnancies;

public sealed class PregnancyCalculationResult
{
    public required PregnancyCalculationBasis Basis { get; init; }
    public required DateTime ReferenceDate { get; init; }
    public required DateTime EstimatedDueDate { get; init; }
    public required int GestationalDay { get; init; }
    public required bool IsOverdue { get; init; }
}

public sealed class PregnancyCalculationException : Exception
{
    public PregnancyCalculationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Pregnancy calculation failed.") => Errors = errors;

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}

public sealed class PregnancyCalculationPolicy
{
    public PregnancyCalculationResult Calculate(
        string species, DateTime? matingDate, DateTime? ovulationDate, DateTime asOfUtc)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        var reference = ovulationDate ?? matingDate;

        if (reference is null)
        {
            errors["ReferenceDate"] = new[] { "Informe a data de cobertura ou a data de ovulação/progesterona." };
            throw new PregnancyCalculationException(errors);
        }

        var futureDateField = matingDate?.Date > asOfUtc.Date
            ? nameof(matingDate)
            : ovulationDate?.Date > asOfUtc.Date
                ? nameof(ovulationDate)
                : null;
        if (futureDateField is not null)
        {
            errors[futureDateField] =
                new[] { "A data de referência não pode ser futura." };
            throw new PregnancyCalculationException(errors);
        }

        var referenceDay = reference.Value.Date;
        var normalizedSpecies = Species.FromString(species).Value;
        var gestationDays = normalizedSpecies == Species.Dog.Value ? 63 : 65;
        var dueDate = DateTime.SpecifyKind(referenceDay.AddDays(gestationDays), DateTimeKind.Utc);
        var gestationalDay = (asOfUtc.Date - referenceDay).Days;

        return new PregnancyCalculationResult
        {
            Basis = ovulationDate.HasValue
                ? PregnancyCalculationBasis.OvulationDate
                : PregnancyCalculationBasis.MatingDate,
            ReferenceDate = DateTime.SpecifyKind(referenceDay, DateTimeKind.Utc),
            EstimatedDueDate = dueDate,
            GestationalDay = gestationalDay,
            IsOverdue = asOfUtc.Date > dueDate.Date
        };
    }
}