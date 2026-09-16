using System.ComponentModel.DataAnnotations;

namespace VetGest.Contracts.Pregnancies;

public enum PregnancyCalculationBasis
{
    OvulationDate,
    MatingDate
}

public sealed class CreatePregnancyRequest
{
    public DateTime? MatingDate { get; set; }
    public DateTime? OvulationDate { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public sealed class PregnancyDto
{
    public Guid Id { get; set; }
    public Guid PetId { get; set; }
    public string Species { get; set; } = string.Empty;
    public DateTime? MatingDate { get; set; }
    public DateTime? OvulationDate { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime EstimatedDueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public PregnancyCalculationSummary Calculation { get; set; } = new();
}

public sealed class PregnancyCalculationSummary
{
    public PregnancyCalculationBasis Basis { get; set; }
    public DateTime ReferenceDate { get; set; }
    public int GestationalDay { get; set; }
    public DateTime EstimatedDueDate { get; set; }
    public bool IsOverdue { get; set; }
    public string? Phase { get; set; }
    public Guid? PhaseId { get; set; }
    public string UncertaintyStatement { get; set; } = string.Empty;
}