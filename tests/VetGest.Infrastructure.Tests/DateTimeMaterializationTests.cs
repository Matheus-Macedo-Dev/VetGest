using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VetGest.Domain.Entities;
using VetGest.Infrastructure.Persistence;

namespace VetGest.Infrastructure.Tests;

public sealed class DateTimeMaterializationTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private VetGestDbContext _dbContext = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        _connection.CreateFunction("GETUTCDATE", () => DateTime.UtcNow);
        _dbContext = CreateContext();
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task Materializes_pregnancy_dates_as_utc()
    {
        var ownerId = "owner-1";
        var pet = new Pet(Guid.NewGuid(), "Luna", "Dog");
        var matingDate = new DateTime(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc);
        var dueDate = new DateTime(2026, 10, 3, 12, 0, 0, DateTimeKind.Utc);
        var pregnancy = new Pregnancy(Guid.NewGuid(), pet.Id, dueDate, matingDate);

        _dbContext.Entry(pet).Property("OwnerId").CurrentValue = ownerId;
        _dbContext.Entry(pregnancy).Property("OwnerId").CurrentValue = ownerId;
        await _dbContext.Pets.AddAsync(pet);
        await _dbContext.Pregnancies.AddAsync(pregnancy);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var result = await new PregnancyRepository(_dbContext)
            .GetCurrentOwnedAsync(pet.Id, ownerId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(DateTimeKind.Utc, result!.MatingDate!.Value.Kind);
        Assert.Equal(DateTimeKind.Utc, result.EstimatedDueDate.Kind);
        Assert.Equal(matingDate, result.MatingDate.Value);
        Assert.Equal(dueDate, result.EstimatedDueDate);
    }

    [Fact]
    public async Task Materializes_diary_entry_date_as_utc()
    {
        var ownerId = "owner-1";
        var pet = new Pet(Guid.NewGuid(), "Luna", "Dog");
        var matingDate = new DateTime(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc);
        var pregnancy = new Pregnancy(
            Guid.NewGuid(),
            pet.Id,
            new DateTime(2026, 10, 3, 12, 0, 0, DateTimeKind.Utc),
            matingDate);
        var entryDate = new DateTime(2026, 8, 20, 12, 0, 0, DateTimeKind.Utc);
        var entry = new PregnancyDiaryEntry(Guid.NewGuid(), pregnancy.Id, entryDate);

        _dbContext.Entry(pet).Property("OwnerId").CurrentValue = ownerId;
        _dbContext.Entry(pregnancy).Property("OwnerId").CurrentValue = ownerId;
        _dbContext.Entry(entry).Property("OwnerId").CurrentValue = ownerId;
        await _dbContext.Pets.AddAsync(pet);
        await _dbContext.Pregnancies.AddAsync(pregnancy);
        await _dbContext.PregnancyDiaryEntries.AddAsync(entry);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var result = await new PregnancyDiaryRepository(_dbContext)
            .ListOwnedAsync(pregnancy.Id, ownerId, CancellationToken.None);

        var materializedEntry = Assert.Single(result);
        Assert.Equal(DateTimeKind.Utc, materializedEntry.EntryDate.Kind);
        Assert.Equal(entryDate, materializedEntry.EntryDate);
    }

    private VetGestDbContext CreateContext() => new(
        new DbContextOptionsBuilder<VetGestDbContext>()
            .UseSqlite(_connection)
            .Options);
}
