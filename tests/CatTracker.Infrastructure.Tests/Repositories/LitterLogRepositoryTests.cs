using CatTracker.Domain.Entities;
using CatTracker.Domain.Enums;
using CatTracker.Infrastructure.Data;
using CatTracker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace CatTracker.Infrastructure.Tests.Repositories;

[TestFixture]
public class LitterLogRepositoryTests
{
    private PostgreSqlContainer _dbContainer = null!;
    private CatTrackerDbContext _context = null!;
    private LitterLogRepository _repository = null!;

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

        _repository = new LitterLogRepository(_context);
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
        _context.LitterLogs.RemoveRange(_context.LitterLogs);
        _context.Cats.RemoveRange(_context.Cats);
        await _context.SaveChangesAsync();
    }

    // Helper: inserts a Cat row required by the LitterLog foreign key constraint
    private async Task<Cat> CreateCatAsync(string name = "Whiskers")
    {
        var cat = Cat.Create(name);
        await _context.Cats.AddAsync(cat);
        await _context.SaveChangesAsync();
        return cat;
    }

    // Verifies that GetSinceAsync returns logs whose LoggedAt is on or after the cutoff,
    // so the >= boundary is correctly inclusive.
    [Test]
    public async Task GetSinceAsync_ReturnLogsOnOrAfterSince()
    {
        var cat = await CreateCatAsync();
        var since = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);

        var logBefore = LitterLog.Create(cat.Id, LitterEntryType.Use, loggedAt: new DateTime(2026, 1, 9, 0, 0, 0, DateTimeKind.Utc));
        var logOnBoundary = LitterLog.Create(cat.Id, LitterEntryType.TopUp, loggedAt: since);
        var logAfter = LitterLog.Create(cat.Id, LitterEntryType.FullChange, loggedAt: new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc));

        await _context.LitterLogs.AddRangeAsync(logBefore, logOnBoundary, logAfter);
        await _context.SaveChangesAsync();

        var result = await _repository.GetSinceAsync(cat.Id, since);

        result.Should().HaveCount(2);
        result.Should().Contain(l => l.Id == logOnBoundary.Id);
        result.Should().Contain(l => l.Id == logAfter.Id);
    }

    // Verifies that logs before the cutoff are excluded, confirming the filter
    // does not accidentally return the full history.
    [Test]
    public async Task GetSinceAsync_ExcludesLogsBeforeSince()
    {
        var cat = await CreateCatAsync();
        var since = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);

        var logBefore = LitterLog.Create(cat.Id, LitterEntryType.Use, loggedAt: new DateTime(2026, 1, 9, 23, 59, 59, DateTimeKind.Utc));
        await _context.LitterLogs.AddAsync(logBefore);
        await _context.SaveChangesAsync();

        var result = await _repository.GetSinceAsync(cat.Id, since);

        result.Should().BeEmpty();
    }

    // Verifies that an empty list is returned (not null, not an exception)
    // when no litter logs exist for the given cat.
    [Test]
    public async Task GetSinceAsync_ReturnsEmpty_WhenNoLogsExist()
    {
        var cat = await CreateCatAsync();

        var result = await _repository.GetSinceAsync(cat.Id, DateTime.UtcNow.AddDays(-7));

        result.Should().BeEmpty();
    }

    // Verifies that the most recently logged entry is returned, confirming the
    // OrderByDescending + FirstOrDefault logic selects the right row.
    [Test]
    public async Task GetLatestAsync_ReturnsMostRecentLog()
    {
        var cat = await CreateCatAsync();

        var older = LitterLog.Create(cat.Id, LitterEntryType.Use, loggedAt: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var newer = LitterLog.Create(cat.Id, LitterEntryType.TopUp, loggedAt: new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc));

        await _context.LitterLogs.AddRangeAsync(older, newer);
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

    // Verifies that GetLatestFullChangeAsync filters by EntryType == FullChange
    // and returns the most recent one, ignoring Use and TopUp entries.
    [Test]
    public async Task GetLatestFullChangeAsync_ReturnsMostRecentFullChange()
    {
        var cat = await CreateCatAsync();

        var use = LitterLog.Create(cat.Id, LitterEntryType.Use, loggedAt: new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc));
        var olderChange = LitterLog.Create(cat.Id, LitterEntryType.FullChange, loggedAt: new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc));
        var newerChange = LitterLog.Create(cat.Id, LitterEntryType.FullChange, loggedAt: new DateTime(2026, 1, 8, 0, 0, 0, DateTimeKind.Utc));

        await _context.LitterLogs.AddRangeAsync(use, olderChange, newerChange);
        await _context.SaveChangesAsync();

        var result = await _repository.GetLatestFullChangeAsync(cat.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(newerChange.Id);
    }

    // Verifies that null is returned when no FullChange entries exist, so the
    // EntryType filter does not accidentally match Use or TopUp logs.
    [Test]
    public async Task GetLatestFullChangeAsync_ReturnsNull_WhenNoFullChangeExists()
    {
        var cat = await CreateCatAsync();

        var use = LitterLog.Create(cat.Id, LitterEntryType.Use, loggedAt: new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc));
        var topUp = LitterLog.Create(cat.Id, LitterEntryType.TopUp, loggedAt: new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc));

        await _context.LitterLogs.AddRangeAsync(use, topUp);
        await _context.SaveChangesAsync();

        var result = await _repository.GetLatestFullChangeAsync(cat.Id);

        result.Should().BeNull();
    }

    // Verifies that AddAsync actually writes the entity to the database,
    // so it survives a fresh query.
    [Test]
    public async Task AddAsync_PersistsLitterLog()
    {
        var cat = await CreateCatAsync();
        var log = LitterLog.Create(cat.Id, LitterEntryType.FullChange, notes: "Weekly clean");

        await _repository.AddAsync(log);

        var persisted = await _context.LitterLogs.FindAsync(log.Id);
        persisted.Should().NotBeNull();
        persisted!.EntryType.Should().Be(LitterEntryType.FullChange);
        persisted.Notes.Should().Be("Weekly clean");
    }

    // Verifies that DeleteAsync actually removes the row from the database.
    [Test]
    public async Task DeleteAsync_RemovesExistingLog()
    {
        var cat = await CreateCatAsync();
        var log = LitterLog.Create(cat.Id, LitterEntryType.Use);
        await _context.LitterLogs.AddAsync(log);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(log.Id);

        var deleted = await _context.LitterLogs.FindAsync(log.Id);
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
