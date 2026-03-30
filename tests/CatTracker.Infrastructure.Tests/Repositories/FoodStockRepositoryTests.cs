using CatTracker.Domain.Entities;
using CatTracker.Domain.Enums;
using CatTracker.Infrastructure.Data;
using CatTracker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace CatTracker.Infrastructure.Tests.Repositories;

[TestFixture]
public class FoodStockRepositoryTests
{
    private PostgreSqlContainer _dbContainer = null!;
    private CatTrackerDbContext _context = null!;
    private FoodStockRepository _repository = null!;

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

        _repository = new FoodStockRepository(_context);
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
        _context.FoodStocks.RemoveRange(_context.FoodStocks);
        _context.Cats.RemoveRange(_context.Cats);
        await _context.SaveChangesAsync();
    }

    // Helper: inserts a Cat row required by the FoodStock foreign key constraint
    private async Task<Cat> CreateCatAsync(string name = "Whiskers")
    {
        var cat = Cat.Create(name);
        await _context.Cats.AddAsync(cat);
        await _context.SaveChangesAsync();
        return cat;
    }

    // Helper: builds a FoodStock associated with the given catId
    private static FoodStock BuildFoodStock(Guid catId, string brand = "BrandA", FoodType type = FoodType.Dry)
    {
        return FoodStock.Create(
            catId: catId,
            foodBrand: brand,
            foodType: type,
            quantityGrams: 2000m,
            dailyUsageGrams: 100m,
            lowStockThresholdDays: 7);
    }

    // Verifies that only food stocks belonging to the requested cat are returned,
    // so the CatId filter in GetByCatAsync is actually applied.
    [Test]
    public async Task GetByCatAsync_ReturnsFoodStocksForCat()
    {
        var cat = await CreateCatAsync();
        var otherCat = await CreateCatAsync("OtherCat");

        var stockForCat = BuildFoodStock(cat.Id, "BrandA");
        var stockForOther = BuildFoodStock(otherCat.Id, "BrandB");

        await _context.FoodStocks.AddRangeAsync(stockForCat, stockForOther);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByCatAsync(cat.Id);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(stockForCat.Id);
    }

    // Verifies that an empty list is returned (not null, not an exception)
    // when no food stocks exist for the given cat.
    [Test]
    public async Task GetByCatAsync_ReturnsEmpty_WhenNoneExist()
    {
        var cat = await CreateCatAsync();

        var result = await _repository.GetByCatAsync(cat.Id);

        result.Should().BeEmpty();
    }

    // Verifies that results are ordered alphabetically by FoodBrand ascending,
    // matching the contract described in the repository spec.
    [Test]
    public async Task GetByCatAsync_ReturnsStocksOrderedByFoodBrandAscending()
    {
        var cat = await CreateCatAsync();

        var stockC = BuildFoodStock(cat.Id, "Ziwi");
        var stockA = BuildFoodStock(cat.Id, "Applaws");
        var stockB = BuildFoodStock(cat.Id, "Orijen");

        await _context.FoodStocks.AddRangeAsync(stockC, stockA, stockB);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByCatAsync(cat.Id);

        result.Select(s => s.FoodBrand).Should().BeInAscendingOrder();
    }

    // Verifies that a stock can be retrieved by its primary key.
    [Test]
    public async Task GetByIdAsync_ReturnsStock_WhenExists()
    {
        var cat = await CreateCatAsync();
        var stock = BuildFoodStock(cat.Id);
        await _context.FoodStocks.AddAsync(stock);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(stock.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(stock.Id);
    }

    // Verifies that null is returned for an unknown id rather than throwing an exception.
    [Test]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    // Verifies that AddAsync actually writes the entity to the database,
    // so it survives a fresh query.
    [Test]
    public async Task AddAsync_PersistsStock()
    {
        var cat = await CreateCatAsync();
        var stock = BuildFoodStock(cat.Id, "NewBrand");

        await _repository.AddAsync(stock);

        var persisted = await _context.FoodStocks.FindAsync(stock.Id);
        persisted.Should().NotBeNull();
        persisted!.FoodBrand.Should().Be("NewBrand");
    }

    // Verifies that UpdateAsync flushes the changed state to the database,
    // so a subsequent read reflects the updated quantity.
    [Test]
    public async Task UpdateAsync_PersistsChanges()
    {
        var cat = await CreateCatAsync();
        var stock = BuildFoodStock(cat.Id);
        await _context.FoodStocks.AddAsync(stock);
        await _context.SaveChangesAsync();

        stock.UpdateStock(500m);
        await _repository.UpdateAsync(stock);

        // Detach to force a real DB read
        _context.Entry(stock).State = EntityState.Detached;
        var updated = await _context.FoodStocks.FindAsync(stock.Id);
        updated!.QuantityGrams.Should().Be(500m);
    }

    // Verifies that DeleteAsync actually removes the row from the database.
    [Test]
    public async Task DeleteAsync_RemovesStock()
    {
        var cat = await CreateCatAsync();
        var stock = BuildFoodStock(cat.Id);
        await _context.FoodStocks.AddAsync(stock);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(stock.Id);

        var deleted = await _context.FoodStocks.FindAsync(stock.Id);
        deleted.Should().BeNull();
    }

    // Verifies the guard clause in DeleteAsync: calling it with an id that
    // does not exist should be a no-op and must not throw an exception.
    [Test]
    public async Task DeleteAsync_DoesNotThrow_WhenNotExists()
    {
        var act = async () => await _repository.DeleteAsync(Guid.NewGuid());

        await act.Should().NotThrowAsync();
    }
}
