using VetGest.Application.Pets;
using VetGest.Application.Pregnancies;
using VetGest.Contracts.Pregnancies;

namespace VetGest.Application.Alerts;

public interface IAlertEvaluationService
{
    Task<TodayAlertsDto?> GetCurrentAlertsAsync(Guid petId, Guid pregnancyId, string ownerId, CancellationToken cancellationToken);
}

public sealed class AlertEvaluationService : IAlertEvaluationService
{
    private const string NoEntriesDisclaimer =
        "Nenhum registro no diário ainda. Adicione um registro para receber alertas.";

    private const string EvaluationDisclaimer =
        "Estes alertas são educativos e não substituem a avaliação de um médico-veterinário. Em caso de sinais de Atenção ou Emergência, entre em contato com seu veterinário.";

    private readonly IPetRepository _petRepository;
    private readonly IPregnancyRepository _pregnancyRepository;
    private readonly IPregnancyDiaryRepository _diaryRepository;
    private readonly IAlertRuleRepository _alertRuleRepository;

    public AlertEvaluationService(
        IPetRepository petRepository,
        IPregnancyRepository pregnancyRepository,
        IPregnancyDiaryRepository diaryRepository,
        IAlertRuleRepository alertRuleRepository)
    {
        _petRepository = petRepository;
        _pregnancyRepository = pregnancyRepository;
        _diaryRepository = diaryRepository;
        _alertRuleRepository = alertRuleRepository;
    }

    public async Task<TodayAlertsDto?> GetCurrentAlertsAsync(
        Guid petId,
        Guid pregnancyId,
        string ownerId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
            throw new ArgumentException("É necessário ter um usuário autenticado.", nameof(ownerId));

        // Both lookups are required: a pregnancy identifier alone must not establish pet ownership.
        var pet = await _petRepository.GetOwnedAsync(petId, ownerId, cancellationToken);
        if (pet is null || !pet.IsActive)
            return null;

        var pregnancy = await _pregnancyRepository.GetOwnedAsync(petId, pregnancyId, ownerId, cancellationToken);
        if (pregnancy is null)
            return null;

        var entries = await _diaryRepository.ListOwnedAsync(pregnancyId, ownerId, cancellationToken);
        var latestEntry = entries.OrderByDescending(entry => entry.EntryDate).FirstOrDefault();
        if (latestEntry is null)
            return new TodayAlertsDto
            {
                HighestSeverity = AlertSeverityLevel.Normal,
                TriggeredAlerts = Array.Empty<TriggeredAlertDto>(),
                EvaluatedEntryDate = null,
                Disclaimer = NoEntriesDisclaimer
            };

        var rules = await _alertRuleRepository.GetActiveForSpeciesAsync(pet.Species, cancellationToken);
        var triggeredAlerts = rules
            .Where(rule => AlertConditionEvaluator.IsTriggered(rule, latestEntry))
            .Select(rule => new TriggeredAlertDto
            {
                Name = rule.Name,
                Severity = ParseSeverity(rule.Severity),
                Description = rule.Description
            })
            .ToArray();

        var highestSeverity = triggeredAlerts.Length == 0
            ? AlertSeverityLevel.Normal
            : triggeredAlerts.Max(alert => alert.Severity);

        return new TodayAlertsDto
        {
            HighestSeverity = highestSeverity,
            TriggeredAlerts = triggeredAlerts,
            EvaluatedEntryDate = latestEntry.EntryDate,
            Disclaimer = EvaluationDisclaimer
        };
    }

    private static AlertSeverityLevel ParseSeverity(string severity) => severity switch
    {
        "Emergency" => AlertSeverityLevel.Emergency,
        "Attention" => AlertSeverityLevel.Attention,
        _ => AlertSeverityLevel.Normal
    };
}
