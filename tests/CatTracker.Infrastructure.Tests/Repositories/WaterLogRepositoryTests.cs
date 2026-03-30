using CatTracker.Domain.Entities;
using CatTracker.Infrastructure.Data;
using CatTracker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace CatTracker.Infrastructure.Tests.Repositories;

[TestFixture]
public class WaterLogRepositoryTests
{
    private PostgreSqlContainer _dbContainer = null!;
    private CatTrackerDbContext _context = null!;
    private WaterLogRepository _repository = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _dbContainer = new PostgreSqlBuilder("postgres:16-alpine").Build();
        await _dbContainer.StartAsync();

        var options = new DbContextOptionsBuilder<CatTrackerDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;

        _context = new CatTrackerDbContext(options);
        await _context.Database.MigrateAsync();

        _repository = new WaterLogRepository(_context);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _context.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        // Clean up test data between tests so each test starts with a known empty state
        _context.WaterLogs.RemoveRange(_context.WaterLogs);
        _context.Cats.RemoveRange(_context.Cats);
        await _context.SaveChangesAsync();
    }

    // Helper: inserts a Cat row required by the WaterLog foreign key constraint
    private async Task<Cat> CreateCatAsync(string name = "Whiskers")
    {
        var cat = Cat.Create(name);
        await _context.Cats.AddAsync(cat);
        await _context.SaveChangesAsync();
        return cat;
    }

    // Verifies that GetRecentAsync returns only the most recent N logs, ordered
    // newest-first, confirming Take(count) and OrderByDescending work together correctly.
    [Test]
    public async Task GetRecentAsync_ReturnsRequestedCount_OrderedNewestFirst()
    {
        var cat = await CreateCatAsync();

        var oldest = WaterLog.Create(cat.Id, cleanedAt: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var middle = WaterLog.Create(cat.Id, cleanedAt: new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc));
        var newest = WaterLog.Create(cat.Id, cleanedAt: new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc));

        await _context.WaterLogs.AddRangeAsync(oldest, middle, newest);
        await _context.SaveChangesAsync();

        var result = await _repository.GetRecentAsync(cat.Id, count: 2);

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(newest.Id);
        result[1].Id.Should().Be(middle.Id);
    }

    // Verifies that GetRecentAsync returns all logs when the total count is less than
    // the requested count, so the Take(count) cap does not truncate short lists.
    [Test]
    public async Task GetRecentAsync_ReturnsAllLogs_WhenFewerThanCount()
    {
        var cat = await CreateCatAsync();

        var log1 = WaterLog.Create(cat.Id, cleanedAt: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var log2 = WaterLog.Create(cat.Id, cleanedAt: new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc));

        await _context.WaterLogs.AddRangeAsync(log1, log2);
        await _context.SaveChangesAsync();

        var result = await _repository.GetRecentAsync(cat.Id, count: 20);

        result.Should().HaveCount(2);
    }

    // Verifies that an empty list is returned (not null, not an exception)
    // when no water logs exist for the given cat.
    [Test]
    public async Task GetRecentAsync_ReturnsEmpty_WhenNoLogsExist()
    {
        var cat = await CreateCatAsync();

        var result = await _repository.GetRecentAsync(cat.Id);

        result.Should().BeEmpty();
    }

    // Verifies that GetLatestAsync returns the most recently cleaned entry, confirming
    // the OrderByDescending + FirstOrDefault logic selects the right row.
    [Test]
    public async Task GetLatestAsync_ReturnsMostRecentLog()
    {
        var cat = await CreateCatAsync();

        var older = WaterLog.Create(cat.Id, cleanedAt: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var newer = WaterLog.Create(cat.Id, cleanedAt: new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc));

        await _context.WaterLogs.AddRangeAsync(older, newer);
        await _context.SaveChangesAsync();

        var result = await _repository.GetLatestAsync(cat.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(newer.Id);
    }

    // Verifies that null is returned when no logs exist for the cat,
    // so callers can safely handle the absence of data.
    [Test]
    public async Task GetLatestAsync_ReturnsNull_WhenNoLogsExist()
    {
        var cat = await CreateCatAsync();

        var result = await _repository.GetLatestAsync(cat.Id);

        result.Should().BeNull();
    }

    // Verifies that AddAsync actually writes the entity to the database,
    // so it survives a fresh query.
    [Test]
    public async Task AddAsync_PersistsWaterLog()
    {
        var cat = await CreateCatAsync();
        var log = WaterLog.Create(cat.Id, notes: "Scrubbed the bowl");

        await _repository.AddAsync(log);

        var persisted = await _context.WaterLogs.FindAsync(log.Id);
        persisted.Should().NotBeNull();
        persisted!.Notes.Should().Be("Scrubbed the bowl");
    }

    // Verifies that DeleteAsync actually removes the row from the database.
    [Test]
    public async Task DeleteAsync_RemovesExistingLog()
    {
        var cat = await CreateCatAsync();
        var log = WaterLog.Create(cat.Id);
        await _context.WaterLogs.AddAsync(log);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(log.Id);

        var deleted = await _context.WaterLogs.FindAsync(log.Id);
        deleted.Should().BeNull();
    }

    // Verifies the guard clause in DeleteAsync: calling it with an id that
    // does not exist should be a no-op and must not throw an exception.
    [Test]
    public async Task DeleteAsync_DoesNotThrow_WhenLogDoesNotExist()
    {
        var act = async () => await _repository.DeleteAsync(Guid.NewGuid());

        await act.Should().NotThrowAsync();
    }
}
