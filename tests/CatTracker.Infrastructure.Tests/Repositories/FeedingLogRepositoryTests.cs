using CatTracker.Domain.Entities;
using CatTracker.Domain.Enums;
using CatTracker.Infrastructure.Data;
using CatTracker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace CatTracker.Infrastructure.Tests.Repositories;

[TestFixture]
public class FeedingLogRepositoryTests
{
    private PostgreSqlContainer _dbContainer = null!;
    private CatTrackerDbContext _context = null!;
    private FeedingLogRepository _repository = null!;

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

        _repository = new FeedingLogRepository(_context);
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
        _context.FeedingLogs.RemoveRange(_context.FeedingLogs);
        _context.Cats.RemoveRange(_context.Cats);
        await _context.SaveChangesAsync();
    }

    // Helper: inserts a Cat row required by the FeedingLog foreign key constraint
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

        var oldest = FeedingLog.Create(cat.Id, "BrandA", FoodType.Dry, 50m, loggedAt: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var middle = FeedingLog.Create(cat.Id, "BrandB", FoodType.Wet, 80m, loggedAt: new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc));
        var newest = FeedingLog.Create(cat.Id, "BrandC", FoodType.Mixed, 60m, loggedAt: new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc));

        await _context.FeedingLogs.AddRangeAsync(oldest, middle, newest);
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

        var log1 = FeedingLog.Create(cat.Id, "BrandA", FoodType.Dry, 50m, loggedAt: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var log2 = FeedingLog.Create(cat.Id, "BrandB", FoodType.Wet, 80m, loggedAt: new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc));

        await _context.FeedingLogs.AddRangeAsync(log1, log2);
        await _context.SaveChangesAsync();

        var result = await _repository.GetRecentAsync(cat.Id, count: 20);

        result.Should().HaveCount(2);
    }

    // Verifies that an empty list is returned (not null, not an exception)
    // when no feeding logs exist for the given cat.
    [Test]
    public async Task GetRecentAsync_ReturnsEmpty_WhenNoLogsExist()
    {
        var cat = await CreateCatAsync();

        var result = await _repository.GetRecentAsync(cat.Id);

        result.Should().BeEmpty();
    }

    // Verifies that GetRecentAsync only returns logs belonging to the requested cat,
    // confirming the Where(f => f.CatId == catId) filter excludes other cats' logs.
    [Test]
    public async Task GetRecentAsync_ReturnsOnlyLogsForSpecifiedCat()
    {
        var cat1 = await CreateCatAsync("Cat1");
        var cat2 = await CreateCatAsync("Cat2");

        var logCat1 = FeedingLog.Create(cat1.Id, "BrandA", FoodType.Dry, 50m);
        var logCat2 = FeedingLog.Create(cat2.Id, "BrandB", FoodType.Wet, 80m);

        await _context.FeedingLogs.AddRangeAsync(logCat1, logCat2);
        await _context.SaveChangesAsync();

        var result = await _repository.GetRecentAsync(cat1.Id);

        result.Should().HaveCount(1);
        result[0].CatId.Should().Be(cat1.Id);
    }

    // Verifies that GetLatestAsync returns the most recently logged entry, confirming
    // the OrderByDescending + FirstOrDefault logic selects the right row.
    [Test]
    public async Task GetLatestAsync_ReturnsMostRecentLog()
    {
        var cat = await CreateCatAsync();

        var older = FeedingLog.Create(cat.Id, "BrandA", FoodType.Dry, 50m, loggedAt: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var newer = FeedingLog.Create(cat.Id, "BrandB", FoodType.Wet, 80m, loggedAt: new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc));

        await _context.FeedingLogs.AddRangeAsync(older, newer);
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

    // Verifies that GetDistinctBrandsAsync deduplicates brand names and returns them
    // sorted alphabetically, confirming Distinct() and OrderBy(b => b) work correctly.
    [Test]
    public async Task GetDistinctBrandsAsync_ReturnsDistinctBrandsAlphabeticallySorted()
    {
        var cat = await CreateCatAsync();

        var log1 = FeedingLog.Create(cat.Id, "ZenBrand", FoodType.Dry, 50m);
        var log2 = FeedingLog.Create(cat.Id, "AppleBrand", FoodType.Wet, 80m);
        var log3 = FeedingLog.Create(cat.Id, "ZenBrand", FoodType.Mixed, 60m); // duplicate brand

        await _context.FeedingLogs.AddRangeAsync(log1, log2, log3);
        await _context.SaveChangesAsync();

        var result = await _repository.GetDistinctBrandsAsync(cat.Id);

        result.Should().HaveCount(2);
        result[0].Should().Be("AppleBrand");
        result[1].Should().Be("ZenBrand");
    }

    // Verifies that an empty list is returned (not null, not an exception)
    // when no feeding logs exist for the cat, so callers can safely enumerate the result.
    [Test]
    public async Task GetDistinctBrandsAsync_ReturnsEmpty_WhenNoLogsExist()
    {
        var cat = await CreateCatAsync();

        var result = await _repository.GetDistinctBrandsAsync(cat.Id);

        result.Should().BeEmpty();
    }

    // Verifies that GetDistinctBrandsAsync only returns brands for the requested cat,
    // confirming the Where filter excludes brands from other cats' logs.
    [Test]
    public async Task GetDistinctBrandsAsync_ReturnsOnlyBrandsForSpecifiedCat()
    {
        var cat1 = await CreateCatAsync("Cat1");
        var cat2 = await CreateCatAsync("Cat2");

        var logCat1 = FeedingLog.Create(cat1.Id, "BrandForCat1", FoodType.Dry, 50m);
        var logCat2 = FeedingLog.Create(cat2.Id, "BrandForCat2", FoodType.Wet, 80m);

        await _context.FeedingLogs.AddRangeAsync(logCat1, logCat2);
        await _context.SaveChangesAsync();

        var result = await _repository.GetDistinctBrandsAsync(cat1.Id);

        result.Should().HaveCount(1);
        result[0].Should().Be("BrandForCat1");
    }

    // Verifies that AddAsync actually writes the entity to the database
    // with all fields preserved, confirming SaveChangesAsync is called.
    [Test]
    public async Task AddAsync_PersistsFeedingLogWithAllFields()
    {
        var cat = await CreateCatAsync();
        var loggedAt = new DateTime(2026, 1, 15, 12, 30, 0, DateTimeKind.Utc);
        var log = FeedingLog.Create(cat.Id, "PremiumBrand", FoodType.Wet, 120m, notes: "Ate everything", loggedAt: loggedAt);

        await _repository.AddAsync(log);

        _context.Entry(log).State = EntityState.Detached;
        var persisted = await _context.FeedingLogs.FindAsync(log.Id);

        persisted.Should().NotBeNull();
        persisted!.CatId.Should().Be(cat.Id);
        persisted.FoodBrand.Should().Be("PremiumBrand");
        persisted.FoodType.Should().Be(FoodType.Wet);
        persisted.AmountGrams.Should().Be(120m);
        persisted.Notes.Should().Be("Ate everything");
        persisted.LoggedAt.Should().Be(loggedAt);
    }

    // Verifies that DeleteAsync actually removes the row from the database.
    [Test]
    public async Task DeleteAsync_RemovesExistingLog()
    {
        var cat = await CreateCatAsync();
        var log = FeedingLog.Create(cat.Id, "BrandA", FoodType.Dry, 50m);
        await _context.FeedingLogs.AddAsync(log);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(log.Id);

        var deleted = await _context.FeedingLogs.FindAsync(log.Id);
        deleted.Should().BeNull();
    }

    // Verifies the guard clause in DeleteAsync: calling it with an id that does not
    // exist must be a no-op and must not throw any exception.
    [Test]
    public async Task DeleteAsync_DoesNotThrow_WhenLogDoesNotExist()
    {
        var act = async () => await _repository.DeleteAsync(Guid.NewGuid());

        await act.Should().NotThrowAsync();
    }
}
