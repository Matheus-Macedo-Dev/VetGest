using VetGest.Application.Pets;
using VetGest.Application.Pregnancies;
using VetGest.Domain.Entities;

namespace VetGest.Application.Tests;

public sealed class ExaminationReminderTests
{
    private static readonly DateTime Today = new(2026, 9, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Dog_and_cat_use_species_specific_confirmation_and_ultrasound_dates()
    {
        var dog = CreatePet("Dog");
        var cat = CreatePet("Cat");
        var dogPregnancy = CreatePregnancy(dog.Id, Today.AddDays(-10));
        var catPregnancy = CreatePregnancy(cat.Id, Today.AddDays(-10));

        var dogReminders = (await CreateService((dog, dogPregnancy)).GetAsync(dog.Id, dogPregnancy.Id, "owner-1", CancellationToken.None))!;
        var catReminders = (await CreateService((cat, catPregnancy)).GetAsync(cat.Id, catPregnancy.Id, "owner-1", CancellationToken.None))!;

        Assert.Equal(Today.AddDays(15).Date, dogReminders.Single(item => item.Code == "pregnancy-confirmation").EstimatedDate!.Value.ToDateTime(TimeOnly.MinValue));
        Assert.Equal(Today.AddDays(11).Date, catReminders.Single(item => item.Code == "pregnancy-confirmation").EstimatedDate!.Value.ToDateTime(TimeOnly.MinValue));
        Assert.Equal(Today.AddDays(18).Date, dogReminders.Single(item => item.Code == "ultrasound").EstimatedDate!.Value.ToDateTime(TimeOnly.MinValue));
        Assert.Equal(Today.AddDays(11).Date, catReminders.Single(item => item.Code == "ultrasound").EstimatedDate!.Value.ToDateTime(TimeOnly.MinValue));
    }

    [Fact]
    public async Task Returns_all_reminders_sorted_by_date_then_stable_code_including_past_items()
    {
        var pet = CreatePet("Dog");
        var pregnancy = CreatePregnancy(pet.Id, Today.AddDays(-70));

        var reminders = (await CreateService((pet, pregnancy)).GetAsync(pet.Id, pregnancy.Id, "owner-1", CancellationToken.None))!;

        Assert.Equal(new[] { "pregnancy-confirmation", "ultrasound", "pre-birth-review", "radiography" }, reminders.Select(item => item.Code));
        Assert.Equal(4, reminders.Count);
        Assert.All(reminders, item => Assert.True(item.IsPast));
    }

    [Fact]
    public async Task Calculates_due_soon_and_past_flags_from_injected_today()
    {
        var pet = CreatePet("Dog");
        var pregnancy = CreatePregnancy(pet.Id, Today.AddDays(-55));

        var reminders = (await CreateService((pet, pregnancy)).GetAsync(pet.Id, pregnancy.Id, "owner-1", CancellationToken.None))!;

        Assert.True(reminders.Single(item => item.Code == "radiography").IsDueSoon);
        Assert.True(reminders.Single(item => item.Code == "pregnancy-confirmation").IsPast);
        Assert.True(reminders.Single(item => item.Code == "ultrasound").IsPast);
    }

    [Fact]
    public async Task Includes_estimated_date_disclaimer_with_veterinary_direction()
    {
        var pet = CreatePet("Cat");
        var pregnancy = CreatePregnancy(pet.Id, Today.AddDays(-10));

        var reminders = (await CreateService((pet, pregnancy)).GetAsync(pet.Id, pregnancy.Id, "owner-1", CancellationToken.None))!;

        Assert.All(reminders, item =>
        {
            Assert.Contains("estimativas", item.Disclaimer, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("veterinário", item.Disclaimer, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public async Task Returns_null_for_unowned_pregnancy_or_inactive_pet()
    {
        var pet = CreatePet("Dog");
        var pregnancy = CreatePregnancy(pet.Id, Today.AddDays(-10));
        var service = CreateService((pet, pregnancy));

        Assert.Null(await service.GetAsync(pet.Id, pregnancy.Id, "owner-2", CancellationToken.None));

        pet.Deactivate();
        Assert.Null(await service.GetAsync(pet.Id, pregnancy.Id, "owner-1", CancellationToken.None));
    }

    private static ExaminationReminderService CreateService((Pet Pet, Pregnancy Pregnancy) owned) =>
        new(
            new FakePetRepository(owned.Pet),
            new FakePregnancyRepository(owned.Pregnancy),
            new PregnancyCalculationPolicy(),
            () => Today);

    private static Pet CreatePet(string species) => new(Guid.NewGuid(), "Luna", species);

    private static Pregnancy CreatePregnancy(Guid petId, DateTime matingDate) =>
        new(Guid.NewGuid(), petId, matingDate.AddDays(63), matingDate);

    private sealed class FakePetRepository : IPetRepository
    {
        private readonly Pet _pet;

        public FakePetRepository(Pet pet) => _pet = pet;
        public Task AddAsync(Pet pet, string ownerId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<Pet>> ListOwnedAsync(string ownerId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Pet>>(new[] { _pet });
        public Task<Pet?> GetOwnedAsync(Guid petId, string ownerId, CancellationToken cancellationToken) =>
            Task.FromResult<Pet?>(petId == _pet.Id && ownerId == "owner-1" ? _pet : null);
    }

    private sealed class FakePregnancyRepository : IPregnancyRepository
    {
        private readonly Pregnancy _pregnancy;

        public FakePregnancyRepository(Pregnancy pregnancy) => _pregnancy = pregnancy;
        public Task AddAsync(Pregnancy pregnancy, string ownerId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Pregnancy?> GetOwnedAsync(Guid petId, Guid pregnancyId, string ownerId, CancellationToken cancellationToken) =>
            Task.FromResult<Pregnancy?>(petId == _pregnancy.PetId && pregnancyId == _pregnancy.Id && ownerId == "owner-1" ? _pregnancy : null);
        public Task<Pregnancy?> GetCurrentOwnedAsync(Guid petId, string ownerId, CancellationToken cancellationToken) => Task.FromResult<Pregnancy?>(null);
        public Task<Phase?> GetPhaseForDayAsync(string species, int gestationalDay, CancellationToken cancellationToken) => Task.FromResult<Phase?>(null);
        public Task<Phase?> GetLastPhaseAsync(string species, CancellationToken cancellationToken) => Task.FromResult<Phase?>(null);
    }
}