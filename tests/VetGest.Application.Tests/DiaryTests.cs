using VetGest.Application.Pets;
using VetGest.Application.Pregnancies;
using VetGest.Contracts.Pregnancies;
using VetGest.Domain.Entities;

namespace VetGest.Application.Tests;

public sealed class PregnancyDiaryServiceTests
{
    private static readonly DateTime EntryDate = new(2026, 9, 3, 15, 30, 0, DateTimeKind.Local);

    [Fact]
    public async Task Creates_partial_entry_with_utc_midnight_and_array_mapping()
    {
        var pet = new Pet(Guid.NewGuid(), "Luna", "Dog");
        var pregnancy = CreatePregnancy(pet.Id);
        var diary = new FakeDiaryRepository();
        var service = CreateService(pet, pregnancy, diary);

        var result = await service.CreateAsync(pet.Id, pregnancy.Id, new CreateDiaryEntryRequest
        {
            EntryDate = EntryDate,
            Notes = "Quiet day",
            Symptoms = new[] { " nesting ", " " },
            PhotoReferences = new[] { "photo-ref" }
        }, "owner-1", CancellationToken.None);

        Assert.Equal(new DateTime(2026, 9, 3, 0, 0, 0, DateTimeKind.Utc), result.EntryDate);
        Assert.Equal(new[] { "nesting" }, result.Symptoms);
        Assert.Equal(new[] { "photo-ref" }, result.PhotoReferences);
        Assert.Null(result.TemperatureCelsius);
        Assert.False(diary.Entries.Single().HasAbnormalSigns());
    }

    [Fact]
    public async Task Rejects_future_date_invalid_appetite_and_bounds()
    {
        var request = new CreateDiaryEntryRequest
        {
            EntryDate = DateTime.UtcNow.Date.AddDays(1),
            Appetite = "Unknown",
            WeightKg = 0,
            TemperatureCelsius = 50,
            Notes = "observation"
        };
        var exception = await Assert.ThrowsAsync<DiaryValidationException>(() =>
            CreateService(new Pet(Guid.NewGuid(), "Luna", "Dog"), CreatePregnancy(Guid.NewGuid()), new FakeDiaryRepository())
                .CreateAsync(Guid.NewGuid(), Guid.NewGuid(), request, "owner-1", CancellationToken.None));

        Assert.Contains(nameof(request.EntryDate), exception.Errors.Keys);
        Assert.Contains(nameof(request.Appetite), exception.Errors.Keys);
        Assert.Contains(nameof(request.WeightKg), exception.Errors.Keys);
        Assert.Contains(nameof(request.TemperatureCelsius), exception.Errors.Keys);
    }

    [Fact]
    public async Task Rejects_date_without_an_observation()
    {
        var pet = new Pet(Guid.NewGuid(), "Luna", "Dog");
        var pregnancy = CreatePregnancy(pet.Id);

        var exception = await Assert.ThrowsAsync<DiaryValidationException>(() =>
            CreateService(pet, pregnancy, new FakeDiaryRepository()).CreateAsync(
                pet.Id, pregnancy.Id, new CreateDiaryEntryRequest { EntryDate = EntryDate }, "owner-1", CancellationToken.None));

        Assert.Contains("Observation", exception.Errors.Keys);
    }

    [Fact]
    public async Task Rejects_duplicate_calendar_day_and_lists_newest_first()
    {
        var pet = new Pet(Guid.NewGuid(), "Luna", "Dog");
        var pregnancy = CreatePregnancy(pet.Id);
        var diary = new FakeDiaryRepository();
        var service = CreateService(pet, pregnancy, diary);
        var request = new CreateDiaryEntryRequest { EntryDate = EntryDate, Notes = "first" };

        await service.CreateAsync(pet.Id, pregnancy.Id, request, "owner-1", CancellationToken.None);
        await Assert.ThrowsAsync<DiaryDuplicateDateException>(() => service.CreateAsync(
            pet.Id, pregnancy.Id, request, "owner-1", CancellationToken.None));

        diary.Entries.Add(new PregnancyDiaryEntry(Guid.NewGuid(), pregnancy.Id,
            DateTime.SpecifyKind(EntryDate.AddDays(1).Date, DateTimeKind.Utc), notes: "newer"));
        var entries = await service.ListAsync(pet.Id, pregnancy.Id, "owner-1", CancellationToken.None);
        Assert.Equal("newer", entries![0].Notes);
    }

    [Fact]
    public async Task Hides_foreign_and_inactive_parent_resources()
    {
        var pet = new Pet(Guid.NewGuid(), "Luna", "Dog");
        var pregnancy = CreatePregnancy(pet.Id);
        var service = CreateService(pet, pregnancy, new FakeDiaryRepository());

        Assert.Null(await service.ListAsync(pet.Id, pregnancy.Id, "owner-2", CancellationToken.None));
        pet.Deactivate();
        Assert.Null(await service.ListAsync(pet.Id, pregnancy.Id, "owner-1", CancellationToken.None));
    }

    private static PregnancyDiaryService CreateService(Pet pet, Pregnancy pregnancy, FakeDiaryRepository diary) =>
        new(new FakePetRepository(pet, "owner-1"), new FakePregnancyRepository(pregnancy, "owner-1"), diary);

    private static Pregnancy CreatePregnancy(Guid petId) =>
        new(Guid.NewGuid(), petId, DateTime.UtcNow.Date.AddDays(50), DateTime.UtcNow.Date.AddDays(-10));

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

    private sealed class FakeDiaryRepository : IPregnancyDiaryRepository
    {
        public List<PregnancyDiaryEntry> Entries { get; } = new();
        public Task AddAsync(PregnancyDiaryEntry entry, string ownerId, CancellationToken cancellationToken) { Entries.Add(entry); return Task.CompletedTask; }
        public Task<bool> ExistsForDateAsync(Guid pregnancyId, DateTime entryDate, string ownerId, CancellationToken cancellationToken) =>
            Task.FromResult(Entries.Any(entry => entry.PregnancyId == pregnancyId && entry.EntryDate == entryDate));
        public Task<IReadOnlyList<PregnancyDiaryEntry>> ListOwnedAsync(Guid pregnancyId, string ownerId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<PregnancyDiaryEntry>>(Entries.Where(entry => entry.PregnancyId == pregnancyId).OrderByDescending(entry => entry.EntryDate).ToArray());
    }
}