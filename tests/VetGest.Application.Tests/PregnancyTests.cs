using VetGest.Application.Pets;
using VetGest.Application.Pregnancies;
using VetGest.Contracts.Pregnancies;
using VetGest.Domain.Entities;

namespace VetGest.Application.Tests;

public sealed class PregnancyCalculationPolicyTests
{
    private static readonly DateTime AsOf = new(2026, 9, 4, 12, 0, 0, DateTimeKind.Utc);
    private readonly PregnancyCalculationPolicy _policy = new();

    [Fact]
    public void Uses_mating_date_for_a_dog_when_ovulation_is_missing()
    {
        var result = _policy.Calculate("Dog", AsOf.AddDays(-10), null, AsOf);

        Assert.Equal(PregnancyCalculationBasis.MatingDate, result.Basis);
        Assert.Equal(63, (result.EstimatedDueDate - result.ReferenceDate).Days);
        Assert.Equal(10, result.GestationalDay);
    }

    [Fact]
    public void Uses_ovulation_date_when_both_dates_are_present()
    {
        var result = _policy.Calculate("Dog", AsOf.AddDays(-30), AsOf.AddDays(-5), AsOf);

        Assert.Equal(PregnancyCalculationBasis.OvulationDate, result.Basis);
        Assert.Equal(5, result.GestationalDay);
        Assert.Equal(AsOf.AddDays(58).Date, result.EstimatedDueDate.Date);
    }

    [Fact]
    public void Uses_cat_default_of_65_days()
    {
        var result = _policy.Calculate("Cat", AsOf.AddDays(-1), null, AsOf);

        Assert.Equal(65, (result.EstimatedDueDate - result.ReferenceDate).Days);
    }

    [Fact]
    public void Rejects_missing_and_future_reference_dates()
    {
        Assert.Throws<PregnancyCalculationException>(() => _policy.Calculate("Dog", null, null, AsOf));
        Assert.Throws<PregnancyCalculationException>(() => _policy.Calculate("Dog", AsOf.AddDays(1), null, AsOf));
    }

    [Fact]
    public void Calculates_day_zero_and_negative_days_deterministically()
    {
        var dayZero = _policy.Calculate("Dog", AsOf, null, AsOf);

        Assert.Equal(0, dayZero.GestationalDay);
        Assert.Throws<PregnancyCalculationException>(() => _policy.Calculate("Dog", AsOf, null, AsOf.AddDays(-1)));
    }

    [Fact]
    public void Marks_past_due_dates_overdue()
    {
        var result = _policy.Calculate("Dog", AsOf.AddDays(-64), null, AsOf);

        Assert.True(result.IsOverdue);
    }
}

public sealed class PregnancyServiceTests
{
    private static readonly DateTime ReferenceDate = DateTime.UtcNow.Date.AddDays(-10);

    [Fact]
    public async Task Selects_species_phase_and_rejects_inactive_or_foreign_pets()
    {
        var ownedPet = new Pet(Guid.NewGuid(), "Luna", "Dog");
        var inactivePet = new Pet(Guid.NewGuid(), "Mia", "Cat");
        inactivePet.Deactivate();
        var pets = new FakePetRepository((ownedPet, "owner-1"), (inactivePet, "owner-1"));
        var repository = new FakePregnancyRepository(new Phase(Guid.NewGuid(), "Initial", "Dog", 0, 14, "d", "f", "m"));
        var service = new PregnancyService(pets, repository, new PregnancyCalculationPolicy());

        var result = await service.CreateAsync(ownedPet.Id, new CreatePregnancyRequest
        {
            MatingDate = ReferenceDate
        }, "owner-1", CancellationToken.None);

        Assert.Equal("Initial", result.Calculation.Phase);
        await Assert.ThrowsAsync<PregnancyNotFoundException>(() => service.CreateAsync(
            inactivePet.Id, new CreatePregnancyRequest { MatingDate = ReferenceDate }, "owner-1", CancellationToken.None));
        Assert.Null(await service.GetCurrentAsync(ownedPet.Id, "owner-2", CancellationToken.None));
    }

    [Fact]
    public async Task Keeps_the_last_phase_when_gestational_day_is_beyond_seeded_range()
    {
        var pet = new Pet(Guid.NewGuid(), "Luna", "Cat");
        var final = new Phase(Guid.NewGuid(), "Birth", "Cat", 65, 72, "d", "f", "m");
        var repository = new FakePregnancyRepository(final);
        var service = new PregnancyService(new FakePetRepository((pet, "owner-1")), repository, new PregnancyCalculationPolicy());

        var result = await service.CreateAsync(pet.Id, new CreatePregnancyRequest
        {
            MatingDate = DateTime.UtcNow.Date.AddDays(-80)
        }, "owner-1", CancellationToken.None);

        Assert.True(result.Calculation.IsOverdue);
        Assert.Equal("Birth", result.Calculation.Phase);
    }

    [Theory]
    [InlineData(14, "Initial")]
    [InlineData(15, "Intermediate")]
    public async Task Selects_phase_at_inclusive_persisted_boundaries(int age, string expectedPhase)
    {
        var pet = new Pet(Guid.NewGuid(), "Luna", "Dog");
        var repository = new FakePregnancyRepository(
            new Phase(Guid.NewGuid(), "Initial", "Dog", 0, 14, "d", "f", "m"),
            new Phase(Guid.NewGuid(), "Intermediate", "Dog", 15, 29, "d", "f", "m"));
        var service = new PregnancyService(new FakePetRepository((pet, "owner-1")), repository, new PregnancyCalculationPolicy());

        var result = await service.CreateAsync(pet.Id, new CreatePregnancyRequest
        {
            MatingDate = DateTime.UtcNow.Date.AddDays(-age)
        }, "owner-1", CancellationToken.None);

        Assert.Equal(expectedPhase, result.Calculation.Phase);
    }

    private sealed class FakePetRepository : IPetRepository
    {
        private readonly (Pet Pet, string OwnerId)[] _pets;

        public FakePetRepository(params (Pet Pet, string OwnerId)[] pets) => _pets = pets;

        public Task AddAsync(Pet pet, string ownerId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<Pet>> ListOwnedAsync(string ownerId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Pet>>(_pets.Where(item => item.OwnerId == ownerId).Select(item => item.Pet).ToArray());
        public Task<Pet?> GetOwnedAsync(Guid petId, string ownerId, CancellationToken cancellationToken) =>
            Task.FromResult(_pets.Where(item => item.Pet.Id == petId && item.OwnerId == ownerId).Select(item => (Pet?)item.Pet).FirstOrDefault());
    }

    private sealed class FakePregnancyRepository : IPregnancyRepository
    {
        private readonly List<Pregnancy> _pregnancies = new();
        private readonly Phase[] _phases;

        public FakePregnancyRepository(params Phase[] phases) => _phases = phases;

        public Task AddAsync(Pregnancy pregnancy, string ownerId, CancellationToken cancellationToken)
        {
            _pregnancies.Add(pregnancy);
            return Task.CompletedTask;
        }

        public Task<Pregnancy?> GetOwnedAsync(Guid petId, Guid pregnancyId, string ownerId, CancellationToken cancellationToken) =>
            Task.FromResult(_pregnancies.FirstOrDefault(pregnancy => pregnancy.Id == pregnancyId && pregnancy.PetId == petId));

        public Task<Pregnancy?> GetCurrentOwnedAsync(Guid petId, string ownerId, CancellationToken cancellationToken) =>
            Task.FromResult(_pregnancies.FirstOrDefault(pregnancy => pregnancy.PetId == petId));

        public Task<Phase?> GetPhaseForDayAsync(string species, int gestationalDay, CancellationToken cancellationToken) =>
            Task.FromResult(_phases.SingleOrDefault(phase => phase.Species == species && phase.ContainsDay(gestationalDay)));

        public Task<Phase?> GetLastPhaseAsync(string species, CancellationToken cancellationToken) =>
            Task.FromResult<Phase?>(_phases.Where(phase => phase.Species == species).OrderByDescending(phase => phase.EndDayGestation).FirstOrDefault());
    }
}