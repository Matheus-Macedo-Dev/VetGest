using VetGest.Application.Pets;
using VetGest.Contracts.Pregnancies;
using VetGest.Domain.Entities;

namespace VetGest.Application.Pregnancies;

public interface IExaminationReminderService
{
    Task<IReadOnlyList<ExaminationReminderDto>?> GetAsync(
        Guid petId,
        Guid pregnancyId,
        string ownerId,
        CancellationToken cancellationToken);
}

public sealed class ExaminationReminderService : IExaminationReminderService
{
    private const string Disclaimer =
        "As datas são estimativas e podem variar; o médico-veterinário deve confirmar o momento adequado. " +
        "Este lembrete não fornece diagnóstico, prescrição ou instruções de procedimento.";

    private readonly IPetRepository _petRepository;
    private readonly IPregnancyRepository _pregnancyRepository;
    private readonly PregnancyCalculationPolicy _calculationPolicy;
    private readonly Func<DateTime> _utcNow;

    public ExaminationReminderService(
        IPetRepository petRepository,
        IPregnancyRepository pregnancyRepository,
        PregnancyCalculationPolicy calculationPolicy,
        Func<DateTime>? utcNow = null)
    {
        _petRepository = petRepository;
        _pregnancyRepository = pregnancyRepository;
        _calculationPolicy = calculationPolicy;
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    public async Task<IReadOnlyList<ExaminationReminderDto>?> GetAsync(
        Guid petId,
        Guid pregnancyId,
        string ownerId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
            throw new ArgumentException("É necessário ter um usuário autenticado.", nameof(ownerId));

        var pet = await _petRepository.GetOwnedAsync(petId, ownerId, cancellationToken);
        if (pet is null || !pet.IsActive)
            return null;

        var pregnancy = await _pregnancyRepository.GetOwnedAsync(petId, pregnancyId, ownerId, cancellationToken);
        if (pregnancy is null)
            return null;

        var today = _utcNow().Date;
        var calculation = _calculationPolicy.Calculate(
            pet.Species,
            pregnancy.MatingDate,
            pregnancy.OvulationDate,
            DateTime.SpecifyKind(today, DateTimeKind.Utc));

        var referenceDate = DateOnly.FromDateTime(calculation.ReferenceDate);
        var matingDate = pregnancy.MatingDate.HasValue
            ? DateOnly.FromDateTime(pregnancy.MatingDate.Value)
            : referenceDate;
        var estimatedDueDate = DateOnly.FromDateTime(calculation.EstimatedDueDate);
        var isDog = pet.Species.Equals("Dog", StringComparison.OrdinalIgnoreCase);
        var confirmationDays = isDog ? 25 : 21;
        var ultrasoundDays = isDog ? 28 : 21;
        var speciesName = isDog ? "cães" : "gatos";
        var reminders = new[]
        {
            CreateReminder(
                "pregnancy-confirmation",
                "Confirmação da gestação",
                $"Para {speciesName}, esta é uma janela educativa aproximada para confirmação da gestação; o médico-veterinário determina o exame apropriado.",
                $"Aproximadamente {confirmationDays} dias após a data de referência para {speciesName}; estimativa.",
                matingDate.AddDays(confirmationDays),
                today),
            CreateReminder(
                "ultrasound",
                "Avaliação por ultrassom",
                $"Para {speciesName}, esta é uma janela educativa aproximada para avaliação por ultrassom; o médico-veterinário determina se o exame é apropriado.",
                $"Aproximadamente {ultrasoundDays} dias após a data de referência para {speciesName}; estimativa.",
                matingDate.AddDays(ultrasoundDays),
                today),
            CreateReminder(
                "radiography",
                "Avaliação por radiografia",
                "A radiografia no fim da gestação depende de decisão do médico-veterinário e pode ser considerada em uma janela aproximada antes do parto.",
                "Janela aproximada: cerca de 7 dias antes da data estimada; estimativa.",
                estimatedDueDate.AddDays(-7),
                today),
            CreateReminder(
                "pre-birth-review",
                "Revisão pré-parto",
                "Uma revisão veterinária antes do parto pode ajudar a orientar o acompanhamento; o médico-veterinário confirma o momento adequado.",
                "Janela aproximada: cerca de 7 dias antes da data estimada; estimativa.",
                estimatedDueDate.AddDays(-7),
                today)
        };

        return reminders
            .OrderBy(reminder => reminder.EstimatedDate)
            .ThenBy(reminder => reminder.Code, StringComparer.Ordinal)
            .ToArray();
    }

    private static ExaminationReminderDto CreateReminder(
        string code,
        string title,
        string description,
        string recommendedWindow,
        DateOnly estimatedDate,
        DateTime today) => new()
    {
        Code = code,
        Title = title,
        Description = description,
        RecommendedWindow = recommendedWindow,
        EstimatedDate = estimatedDate,
        IsPast = estimatedDate < DateOnly.FromDateTime(today),
        IsDueSoon = estimatedDate >= DateOnly.FromDateTime(today) &&
            estimatedDate <= DateOnly.FromDateTime(today).AddDays(7),
        Disclaimer = Disclaimer
    };
}