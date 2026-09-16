using VetGest.Application.Content;
using VetGest.Application.Pets;
using VetGest.Application.Pregnancies;
using VetGest.Domain.Entities;

namespace VetGest.Application.Tests;

public sealed class PregnancyCareTests
{
    [Theory]
    [InlineData("Dog", 0, "Initial")]
    [InlineData("Dog", 14, "Initial")]
    [InlineData("Dog", 15, "Intermediate")]
    [InlineData("Cat", 30, "Intermediate")]
    [InlineData("Cat", 31, "Growth")]
    [InlineData("Cat", 64, "Final")]
    [InlineData("Cat", 65, "Birth")]
    public async Task Uses_species_specific_persisted_phase_boundaries(
        string species,
        int gestationalDay,
        string expectedPhase)
    {
        var pet = new Pet(Guid.NewGuid(), "Luna", species);
        var pregnancy = CreatePregnancy(pet.Id, gestationalDay);
        var repository = new FakePregnancyRepository(
            pregnancy,
            CreatePhases("Dog", (0, 14, "Initial"), (15, 29, "Intermediate"))
                .Concat(CreatePhases("Cat", (0, 14, "Initial"), (15, 30, "Intermediate"), (31, 48, "Growth"), (49, 64, "Final"), (65, 72, "Birth")))
                .ToArray());
        var service = CreateService((pet, "owner-1"), repository);

        var result = await service.GetAsync(pet.Id, pregnancy.Id, "owner-1", CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result!.IsAvailable);
        Assert.Equal(species, result.Species);
        Assert.Equal(expectedPhase, result.Phase);
        Assert.Equal("Reference", result.PublicationStatus);
        Assert.Equal("Unreviewed", result.ReviewStatus);
        Assert.Null(result.Reviewer);
        Assert.Null(result.ReviewDate);
    }

    [Fact]
    public async Task Returns_safe_unpublished_state_when_catalog_content_is_missing()
    {
        var pet = new Pet(Guid.NewGuid(), "Luna", "Dog");
        var pregnancy = CreatePregnancy(pet.Id, 0);
        var phase = new Phase(Guid.NewGuid(), "Initial", "Dog", 0, 14, "fase", "feto", "mãe");
        var repository = new FakePregnancyRepository(pregnancy, phase);
        var service = CreateService((pet, "owner-1"), repository, new EmptyCatalog());

        var result = await service.GetAsync(pet.Id, pregnancy.Id, "owner-1", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result!.IsAvailable);
        Assert.Equal("Unpublished", result.PublicationStatus);
        Assert.Equal("Unreviewed", result.ReviewStatus);
        Assert.Empty(result.GuidanceItems);
        Assert.Contains("Não há conteúdo publicado", result.Disclaimer);
    }

    [Fact]
    public async Task Rejects_unrelated_tutor_even_when_pregnancy_id_is_known()
    {
        var pet = new Pet(Guid.NewGuid(), "Luna", "Dog");
        var pregnancy = CreatePregnancy(pet.Id, 0);
        var repository = new FakePregnancyRepository(pregnancy, CreatePhases("Dog", (0, 14, "Initial")).ToArray());
        var service = CreateService((pet, "owner-1"), repository);

        var result = await service.GetAsync(pet.Id, pregnancy.Id, "owner-2", CancellationToken.None);

        Assert.Null(result);
        Assert.False(repository.PregnancyLookupWasCalled);
    }

    [Fact]
    public async Task Rejects_empty_owner_id_before_accessing_resources()
    {
        var pet = new Pet(Guid.NewGuid(), "Luna", "Dog");
        var repository = new FakePregnancyRepository(CreatePregnancy(pet.Id, 0));
        var service = CreateService((pet, "owner-1"), repository);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetAsync(
            pet.Id, Guid.NewGuid(), "", CancellationToken.None));
    }

    [Fact]
    public void Reference_content_is_portuguese_and_guidance_is_not_prescriptive()
    {
        var content = new PhaseCareContentCatalog().Find("Cat", "Growth");

        Assert.NotNull(content);
        Assert.Equal("Reference", content!.PublicationStatus);
        Assert.Equal("Unreviewed", content.ReviewStatus);
        Assert.Null(content.Reviewer);
        Assert.Null(content.ReviewDate);
        Assert.Contains("equipe veterinária", content.GuidanceItems.Single(item => item.Title == "Nutrição").Guidance);
        Assert.DoesNotContain(content.GuidanceItems, item => ContainsProhibitedWording(item.Guidance));
        Assert.Contains("não substitui", content.Disclaimer, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("diagnosticar", content.Disclaimer, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("prescrever", content.Disclaimer, StringComparison.OrdinalIgnoreCase);
    }

    private static PregnancyCareService CreateService(
        (Pet Pet, string OwnerId) ownedPet,
        FakePregnancyRepository repository,
        IPhaseCareContentCatalog? catalog = null) =>
        new(
            new FakePetRepository(ownedPet),
            repository,
            new PregnancyCalculationPolicy(),
            catalog ?? new PhaseCareContentCatalog());

    private static Pregnancy CreatePregnancy(Guid petId, int gestationalDay) =>
        new(Guid.NewGuid(), petId, DateTime.UtcNow.Date.AddDays(53 - gestationalDay), DateTime.UtcNow.Date.AddDays(-gestationalDay));

    private static IEnumerable<Phase> CreatePhases(
        string species,
        params (int Start, int End, string Name)[] phases) =>
        phases.Select(phase => new Phase(
            Guid.NewGuid(), phase.Name, species, phase.Start, phase.End, "fase", "feto", "mãe"));

    private static bool ContainsProhibitedWording(string guidance) =>
        guidance.Contains("diagnóst", StringComparison.OrdinalIgnoreCase) ||
        guidance.Contains("prescri", StringComparison.OrdinalIgnoreCase) ||
        guidance.Contains("medica", StringComparison.OrdinalIgnoreCase) ||
        guidance.Contains("tratamento", StringComparison.OrdinalIgnoreCase);

    private sealed class EmptyCatalog : IPhaseCareContentCatalog
    {
        public PhaseCareContent? Find(string species, string phase) => null;
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
        private readonly Pregnancy _pregnancy;
        private readonly Phase[] _phases;
        public bool PregnancyLookupWasCalled { get; private set; }

        public FakePregnancyRepository(Pregnancy pregnancy, params Phase[] phases)
        {
            _pregnancy = pregnancy;
            _phases = phases;
        }

        public Task AddAsync(Pregnancy pregnancy, string ownerId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Pregnancy?> GetOwnedAsync(Guid petId, Guid pregnancyId, string ownerId, CancellationToken cancellationToken)
        {
            PregnancyLookupWasCalled = true;
            return Task.FromResult<Pregnancy?>(_pregnancy.PetId == petId && _pregnancy.Id == pregnancyId && ownerId == "owner-1" ? _pregnancy : null);
        }
        public Task<Pregnancy?> GetCurrentOwnedAsync(Guid petId, string ownerId, CancellationToken cancellationToken) => Task.FromResult<Pregnancy?>(null);
        public Task<Phase?> GetPhaseForDayAsync(string species, int gestationalDay, CancellationToken cancellationToken) =>
            Task.FromResult(_phases.SingleOrDefault(phase => phase.Species == species && phase.ContainsDay(gestationalDay)));
        public Task<Phase?> GetLastPhaseAsync(string species, CancellationToken cancellationToken) =>
            Task.FromResult<Phase?>(_phases.Where(phase => phase.Species == species).OrderByDescending(phase => phase.EndDayGestation).FirstOrDefault());
    }
}