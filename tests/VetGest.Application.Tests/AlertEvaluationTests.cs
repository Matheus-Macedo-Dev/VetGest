using VetGest.Application.Alerts;
using VetGest.Application.Pets;
using VetGest.Application.Pregnancies;
using VetGest.Contracts.Pregnancies;
using VetGest.Domain.Entities;

namespace VetGest.Application.Tests;

public sealed class AlertEvaluationTests
{
    [Theory]
    [InlineData("Cat")]
    [InlineData("Dog")]
    public async Task Only_evaluates_rules_for_the_pets_own_species(string species)
    {
        var pet = new Pet(Guid.NewGuid(), "Luna", species);
        var pregnancy = CreatePregnancy(pet.Id);
        var entry = CreateEntry(pregnancy.Id, temperature: 41m);
        var dogRule = TemperatureRule("Dog", "Very High Temperature", "Emergency", min: 40.5m);
        var catRule = TemperatureRule("Cat", "Very High Temperature", "Emergency", min: 39.8m);
        var service = CreateService(pet, pregnancy, new[] { entry }, new[] { dogRule, catRule });

        var result = await service.GetCurrentAlertsAsync(pet.Id, pregnancy.Id, "owner-1", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result!.TriggeredAlerts);
        Assert.Equal(species, pet.Species);
    }

    [Fact]
    public async Task Dog_temperature_of_40_triggers_high_temperature_attention()
    {
        var pet = new Pet(Guid.NewGuid(), "Luna", "Dog");
        var pregnancy = CreatePregnancy(pet.Id);
        var entry = CreateEntry(pregnancy.Id, temperature: 40.0m);
        var rules = new[]
        {
            TemperatureRule("Dog", "High Temperature", "Attention", min: 39.5m, max: 40.5m),
            TemperatureRule("Dog", "Very High Temperature", "Emergency", min: 40.5m)
        };
        var service = CreateService(pet, pregnancy, new[] { entry }, rules);

        var result = await service.GetCurrentAlertsAsync(pet.Id, pregnancy.Id, "owner-1", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains(result!.TriggeredAlerts, alert => alert.Name == "High Temperature");
        Assert.Equal(AlertSeverityLevel.Attention, result.HighestSeverity);
    }

    [Fact]
    public async Task Dog_temperature_of_41_triggers_very_high_temperature_emergency()
    {
        var pet = new Pet(Guid.NewGuid(), "Luna", "Dog");
        var pregnancy = CreatePregnancy(pet.Id);
        var entry = CreateEntry(pregnancy.Id, temperature: 41.0m);
        var rules = new[]
        {
            TemperatureRule("Dog", "High Temperature", "Attention", min: 39.5m, max: 40.5m),
            TemperatureRule("Dog", "Very High Temperature", "Emergency", min: 40.5m)
        };
        var service = CreateService(pet, pregnancy, new[] { entry }, rules);

        var result = await service.GetCurrentAlertsAsync(pet.Id, pregnancy.Id, "owner-1", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains(result!.TriggeredAlerts, alert => alert.Name == "Very High Temperature");
        Assert.Equal(AlertSeverityLevel.Emergency, result.HighestSeverity);
    }

    [Fact]
    public async Task Cat_temperature_of_exactly_39_8_resolves_to_emergency_at_the_boundary()
    {
        var pet = new Pet(Guid.NewGuid(), "Luna", "Cat");
        var pregnancy = CreatePregnancy(pet.Id);
        var entry = CreateEntry(pregnancy.Id, temperature: 39.8m);
        var rules = new[]
        {
            TemperatureRule("Cat", "High Temperature", "Attention", min: 39.0m, max: 39.8m),
            TemperatureRule("Cat", "Very High Temperature", "Emergency", min: 39.8m)
        };
        var service = CreateService(pet, pregnancy, new[] { entry }, rules);

        var result = await service.GetCurrentAlertsAsync(pet.Id, pregnancy.Id, "owner-1", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(AlertSeverityLevel.Emergency, result!.HighestSeverity);
    }

    [Fact]
    public async Task Decreased_appetite_triggers_attention_and_normal_appetite_does_not()
    {
        var pet = new Pet(Guid.NewGuid(), "Luna", "Dog");
        var pregnancy = CreatePregnancy(pet.Id);
        var rule = AppetiteRule("Dog", "Decreased Appetite", "Attention", "decreased");
        var decreasedEntry = CreateEntry(pregnancy.Id, appetite: "Decreased");
        var normalEntry = CreateEntry(pregnancy.Id, appetite: "Normal");

        var decreasedResult = await CreateService(pet, pregnancy, new[] { decreasedEntry }, new[] { rule })
            .GetCurrentAlertsAsync(pet.Id, pregnancy.Id, "owner-1", CancellationToken.None);
        var normalResult = await CreateService(pet, pregnancy, new[] { normalEntry }, new[] { rule })
            .GetCurrentAlertsAsync(pet.Id, pregnancy.Id, "owner-1", CancellationToken.None);

        Assert.Contains(decreasedResult!.TriggeredAlerts, alert => alert.Name == "Decreased Appetite");
        Assert.DoesNotContain(normalResult!.TriggeredAlerts, alert => alert.Name == "Decreased Appetite");
    }

    [Fact]
    public async Task Discharge_symptom_triggers_emergency()
    {
        var pet = new Pet(Guid.NewGuid(), "Luna", "Dog");
        var pregnancy = CreatePregnancy(pet.Id);
        var entry = CreateEntry(pregnancy.Id, symptoms: "discharge");
        var rule = SymptomsRule("Dog", "Abnormal Discharge", "Emergency", "discharge,bleeding");
        var service = CreateService(pet, pregnancy, new[] { entry }, new[] { rule });

        var result = await service.GetCurrentAlertsAsync(pet.Id, pregnancy.Id, "owner-1", CancellationToken.None);

        Assert.Contains(result!.TriggeredAlerts, alert => alert.Name == "Abnormal Discharge");
        Assert.Equal(AlertSeverityLevel.Emergency, result.HighestSeverity);
    }

    [Fact]
    public async Task Lethargy_symptom_triggers_emergency()
    {
        var pet = new Pet(Guid.NewGuid(), "Luna", "Dog");
        var pregnancy = CreatePregnancy(pet.Id);
        var entry = CreateEntry(pregnancy.Id, symptoms: "lethargy");
        var rule = SymptomsRule("Dog", "Severe Lethargy", "Emergency", "lethargy,unresponsive");
        var service = CreateService(pet, pregnancy, new[] { entry }, new[] { rule });

        var result = await service.GetCurrentAlertsAsync(pet.Id, pregnancy.Id, "owner-1", CancellationToken.None);

        Assert.Contains(result!.TriggeredAlerts, alert => alert.Name == "Severe Lethargy");
        Assert.Equal(AlertSeverityLevel.Emergency, result.HighestSeverity);
    }

    [Fact]
    public async Task Inactive_rules_are_excluded_from_evaluation()
    {
        var pet = new Pet(Guid.NewGuid(), "Luna", "Dog");
        var pregnancy = CreatePregnancy(pet.Id);
        var entry = CreateEntry(pregnancy.Id, temperature: 41.0m);
        var activeRules = new[] { TemperatureRule("Dog", "High Temperature", "Attention", min: 39.5m, max: 40.5m) };
        var repository = new FakeAlertRuleRepository(activeRules);
        var service = CreateService(pet, pregnancy, new[] { entry }, repository);

        var result = await service.GetCurrentAlertsAsync(pet.Id, pregnancy.Id, "owner-1", CancellationToken.None);

        Assert.DoesNotContain(result!.TriggeredAlerts, alert => alert.Name == "Very High Temperature");
    }

    [Fact]
    public async Task Multiple_simultaneous_matches_report_the_worst_severity()
    {
        var pet = new Pet(Guid.NewGuid(), "Luna", "Dog");
        var pregnancy = CreatePregnancy(pet.Id);
        var entry = CreateEntry(pregnancy.Id, temperature: 40.0m, appetite: "Decreased");
        var rules = new[]
        {
            TemperatureRule("Dog", "High Temperature", "Attention", min: 39.5m, max: 40.5m),
            AppetiteRule("Dog", "Decreased Appetite", "Attention", "decreased"),
            SymptomsRule("Dog", "Abnormal Discharge", "Emergency", "discharge,bleeding")
        };
        var entryWithDischarge = CreateEntry(pregnancy.Id, temperature: 40.0m, appetite: "Decreased", symptoms: "discharge");
        var service = CreateService(pet, pregnancy, new[] { entryWithDischarge }, rules);

        var result = await service.GetCurrentAlertsAsync(pet.Id, pregnancy.Id, "owner-1", CancellationToken.None);

        Assert.Equal(3, result!.TriggeredAlerts.Count);
        Assert.Equal(AlertSeverityLevel.Emergency, result.HighestSeverity);
    }

    [Fact]
    public async Task No_diary_entries_returns_normal_with_empty_alerts_and_a_disclaimer()
    {
        var pet = new Pet(Guid.NewGuid(), "Luna", "Dog");
        var pregnancy = CreatePregnancy(pet.Id);
        var service = CreateService(pet, pregnancy, Array.Empty<PregnancyDiaryEntry>(), Array.Empty<AlertRule>());

        var result = await service.GetCurrentAlertsAsync(pet.Id, pregnancy.Id, "owner-1", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(AlertSeverityLevel.Normal, result!.HighestSeverity);
        Assert.Empty(result.TriggeredAlerts);
        Assert.Null(result.EvaluatedEntryDate);
        Assert.False(string.IsNullOrWhiteSpace(result.Disclaimer));
    }

    [Fact]
    public async Task Pregnancy_not_owned_by_the_calling_tutor_returns_null()
    {
        var pet = new Pet(Guid.NewGuid(), "Luna", "Dog");
        var pregnancy = CreatePregnancy(pet.Id);
        var service = CreateService(pet, pregnancy, Array.Empty<PregnancyDiaryEntry>(), Array.Empty<AlertRule>());

        var result = await service.GetCurrentAlertsAsync(pet.Id, pregnancy.Id, "owner-2", CancellationToken.None);

        Assert.Null(result);
    }

    private static AlertEvaluationService CreateService(
        Pet pet,
        Pregnancy pregnancy,
        IReadOnlyList<PregnancyDiaryEntry> entries,
        IReadOnlyList<AlertRule> rules) =>
        CreateService(pet, pregnancy, entries, new FakeAlertRuleRepository(rules));

    private static AlertEvaluationService CreateService(
        Pet pet,
        Pregnancy pregnancy,
        IReadOnlyList<PregnancyDiaryEntry> entries,
        FakeAlertRuleRepository ruleRepository) =>
        new(
            new FakePetRepository(pet, "owner-1"),
            new FakePregnancyRepository(pregnancy, "owner-1"),
            new FakeDiaryRepository(entries),
            ruleRepository);

    private static Pregnancy CreatePregnancy(Guid petId) =>
        new(Guid.NewGuid(), petId, DateTime.UtcNow.Date.AddDays(50), DateTime.UtcNow.Date.AddDays(-10));

    private static PregnancyDiaryEntry CreateEntry(
        Guid pregnancyId,
        decimal? temperature = null,
        string? appetite = null,
        string? symptoms = null) =>
        new(Guid.NewGuid(), pregnancyId, DateTime.UtcNow.Date, temperature: temperature, appetite: appetite, symptomsList: symptoms);

    private static AlertRule TemperatureRule(string species, string name, string severity, decimal? min = null, decimal? max = null)
    {
        var conditions = (min, max) switch
        {
            (not null, not null) => $"{{\"temperature_min\": {min}, \"temperature_max\": {max}}}",
            (not null, null) => $"{{\"temperature_min\": {min}}}",
            (null, not null) => $"{{\"temperature_max\": {max}}}",
            _ => throw new ArgumentException("At least one of min or max is required.")
        };
        return new AlertRule(Guid.NewGuid(), name, species, severity, $"{name} description.", conditions);
    }

    private static AlertRule AppetiteRule(string species, string name, string severity, string appetite) =>
        new(Guid.NewGuid(), name, species, severity, $"{name} description.", $"{{\"appetite\": \"{appetite}\"}}");

    private static AlertRule SymptomsRule(string species, string name, string severity, string keywords) =>
        new(Guid.NewGuid(), name, species, severity, $"{name} description.", keywords);

    private sealed class FakePetRepository(Pet pet, string expectedOwnerId) : IPetRepository
    {
        public Task AddAsync(Pet pet, string ownerId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<Pet>> ListOwnedAsync(string ownerId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Pet>>(ownerId == expectedOwnerId ? new[] { pet } : Array.Empty<Pet>());
        public Task<Pet?> GetOwnedAsync(Guid petId, string ownerId, CancellationToken cancellationToken) =>
            Task.FromResult<Pet?>(pet.Id == petId && expectedOwnerId == ownerId ? pet : null);
    }

    private sealed class FakePregnancyRepository(Pregnancy pregnancy, string expectedOwnerId) : IPregnancyRepository
    {
        public Task AddAsync(Pregnancy pregnancy, string ownerId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Pregnancy?> GetOwnedAsync(Guid petId, Guid pregnancyId, string ownerId, CancellationToken cancellationToken) =>
            Task.FromResult<Pregnancy?>(pregnancy.PetId == petId && pregnancy.Id == pregnancyId && expectedOwnerId == ownerId ? pregnancy : null);
        public Task<Pregnancy?> GetCurrentOwnedAsync(Guid petId, string ownerId, CancellationToken cancellationToken) => Task.FromResult<Pregnancy?>(null);
        public Task<Phase?> GetPhaseForDayAsync(string species, int gestationalDay, CancellationToken cancellationToken) => Task.FromResult<Phase?>(null);
        public Task<Phase?> GetLastPhaseAsync(string species, CancellationToken cancellationToken) => Task.FromResult<Phase?>(null);
    }

    private sealed class FakeDiaryRepository(IReadOnlyList<PregnancyDiaryEntry> entries) : IPregnancyDiaryRepository
    {
        public Task AddAsync(PregnancyDiaryEntry entry, string ownerId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> ExistsForDateAsync(Guid pregnancyId, DateTime entryDate, string ownerId, CancellationToken cancellationToken) =>
            Task.FromResult(false);
        public Task<IReadOnlyList<PregnancyDiaryEntry>> ListOwnedAsync(Guid pregnancyId, string ownerId, CancellationToken cancellationToken) =>
            Task.FromResult(entries);
    }

    private sealed class FakeAlertRuleRepository(IReadOnlyList<AlertRule> activeRules) : IAlertRuleRepository
    {
        public Task<IReadOnlyList<AlertRule>> GetActiveForSpeciesAsync(string species, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<AlertRule>>(activeRules.Where(rule => rule.Species == species && rule.IsActive).ToArray());
    }
}
