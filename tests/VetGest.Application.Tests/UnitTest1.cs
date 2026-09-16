using VetGest.Application.Pets;
using VetGest.Contracts.Pets;
using VetGest.Domain.Entities;

namespace VetGest.Application.Tests;

public sealed class PetServiceTests
{
    [Fact]
    public async Task CreateAsync_creates_a_valid_dog_for_the_authenticated_owner()
    {
        var repository = new FakePetRepository();
        var service = new PetService(repository);

        var result = await service.CreateAsync(new CreatePetRequest
        {
            Name = "Luna",
            Species = "Dog",
            Breed = "Labrador",
            CurrentWeight = 24.5m,
            DateOfBirth = new DateTime(2021, 5, 1)
        }, "owner-1", CancellationToken.None);

        Assert.Equal("Luna", result.Name);
        Assert.Equal("Dog", result.Species);
        Assert.Equal("owner-1", repository.OwnerId);
    }

    [Theory]
    [InlineData("", "Dog")]
    [InlineData("Luna", "Rabbit")]
    public async Task CreateAsync_rejects_invalid_name_or_species(string name, string species)
    {
        var service = new PetService(new FakePetRepository());

        await Assert.ThrowsAsync<PetValidationException>(() => service.CreateAsync(new CreatePetRequest
        {
            Name = name,
            Species = species
        }, "owner-1", CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_rejects_non_positive_weight_and_future_birth_date()
    {
        var service = new PetService(new FakePetRepository());

        var exception = await Assert.ThrowsAsync<PetValidationException>(() => service.CreateAsync(new CreatePetRequest
        {
            Name = "Luna",
            Species = "Cat",
            CurrentWeight = 0,
            DateOfBirth = DateTime.UtcNow.Date.AddDays(1)
        }, "owner-1", CancellationToken.None));

        Assert.Contains(nameof(CreatePetRequest.CurrentWeight), exception.Errors.Keys);
        Assert.Contains(nameof(CreatePetRequest.DateOfBirth), exception.Errors.Keys);
    }

    [Fact]
    public async Task GetAsync_returns_null_for_a_pet_owned_by_another_user()
    {
        var repository = new FakePetRepository();
        var service = new PetService(repository);
        var pet = await service.CreateAsync(new CreatePetRequest { Name = "Luna", Species = "Dog" }, "owner-1", CancellationToken.None);

        var result = await service.GetAsync(pet.Id, "owner-2", CancellationToken.None);

        Assert.Null(result);
    }

    private sealed class FakePetRepository : IPetRepository
    {
        private readonly List<(Pet Pet, string OwnerId)> _pets = new();
        public string? OwnerId { get; private set; }

        public Task AddAsync(Pet pet, string ownerId, CancellationToken cancellationToken)
        {
            OwnerId = ownerId;
            _pets.Add((pet, ownerId));
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Pet>> ListOwnedAsync(string ownerId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Pet>>(_pets.Where(item => item.OwnerId == ownerId).Select(item => item.Pet).ToArray());

        public Task<Pet?> GetOwnedAsync(Guid petId, string ownerId, CancellationToken cancellationToken)
        {
            var match = _pets
                .Where(item => item.Pet.Id == petId && item.OwnerId == ownerId)
                .Select(item => (Pet?)item.Pet)
                .FirstOrDefault();
            return Task.FromResult(match);
        }
    }
}
