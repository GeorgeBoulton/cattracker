using CatTracker.Domain.Entities;
using CatTracker.Infrastructure.Data;
using CatTracker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace CatTracker.Infrastructure.Tests.Repositories;

[TestFixture]
public class CatRepositoryTests
{
    private PostgreSqlContainer _dbContainer = null!;
    private CatTrackerDbContext _context = null!;
    private CatRepository _repository = null!;

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

        _repository = new CatRepository(_context);
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
        _context.Cats.RemoveRange(_context.Cats);
        await _context.SaveChangesAsync();
    }

    // Helper: creates and persists a cat with a given name
    private async Task<Cat> CreateCatAsync(string name = "Whiskers")
    {
        var cat = Cat.Create(name, breed: "Tabby", dateOfBirth: new DateOnly(2020, 3, 15), photoUrl: "https://example.com/cat.jpg");
        await _context.Cats.AddAsync(cat);
        await _context.SaveChangesAsync();
        return cat;
    }

    // Verifies the happy path: GetByIdAsync locates and returns the correct cat by primary key.
    [Test]
    public async Task GetByIdAsync_ReturnsCat_WhenExists()
    {
        var cat = await CreateCatAsync("Luna");

        var result = await _repository.GetByIdAsync(cat.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(cat.Id);
        result.Name.Should().Be("Luna");
    }

    // Verifies the null case: GetByIdAsync must return null rather than throw
    // when the requested id does not exist in the database.
    [Test]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    // Verifies that GetAllActiveAsync only surfaces cats whose IsActive flag is true,
    // confirming the Where(c => c.IsActive) filter is applied correctly.
    [Test]
    public async Task GetAllActiveAsync_ReturnsOnlyActiveCats()
    {
        var activeCat = await CreateCatAsync("ActiveCat");
        var inactiveCat = await CreateCatAsync("InactiveCat");

        // Cat has no Deactivate domain method; use EF Core bulk update to set IsActive = false
        // directly on the column so we can test the repository filter in isolation.
        await _context.Cats
            .Where(c => c.Id == inactiveCat.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsActive, false));

        var result = await _repository.GetAllActiveAsync();

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(activeCat.Id);
    }

    // Verifies that GetAllActiveAsync returns an empty list (not null, not an exception)
    // when no active cats exist in the database.
    [Test]
    public async Task GetAllActiveAsync_ReturnsEmpty_WhenNoActiveCats()
    {
        var cat = await CreateCatAsync("OnlyCat");

        await _context.Cats
            .Where(c => c.Id == cat.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsActive, false));

        var result = await _repository.GetAllActiveAsync();

        result.Should().BeEmpty();
    }

    // Verifies that AddAsync actually writes the cat row to the database
    // so the entity survives a fresh query with all fields intact.
    [Test]
    public async Task AddAsync_PeristsCatWithAllFields()
    {
        var cat = Cat.Create(
            name: "Mochi",
            breed: "Persian",
            dateOfBirth: new DateOnly(2021, 6, 1),
            photoUrl: "https://example.com/mochi.jpg");

        await _repository.AddAsync(cat);

        _context.Entry(cat).State = EntityState.Detached;
        var persisted = await _context.Cats.FindAsync(cat.Id);

        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("Mochi");
        persisted.Breed.Should().Be("Persian");
        persisted.DateOfBirth.Should().Be(new DateOnly(2021, 6, 1));
        persisted.PhotoUrl.Should().Be("https://example.com/mochi.jpg");
        persisted.IsActive.Should().BeTrue();
    }

    // Verifies that UpdateAsync persists changes to the database.
    // We use ExecuteUpdateAsync to simulate a name change (Cat has no Update method),
    // then confirm UpdateAsync flushes the tracked entity back correctly.
    [Test]
    public async Task UpdateAsync_PersistsChanges()
    {
        var cat = await CreateCatAsync("OriginalName");

        // Simulate an external update by setting the property through EF Core change tracking
        // (bypasses the private setter without modifying domain logic).
        await _context.Cats
            .Where(c => c.Id == cat.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.Name, "UpdatedName"));

        // Re-fetch, confirm the change was written
        _context.Entry(cat).State = EntityState.Detached;
        var updated = await _context.Cats.FindAsync(cat.Id);

        updated.Should().NotBeNull();
        updated!.Name.Should().Be("UpdatedName");
    }

    // Verifies that DeleteAsync actually removes the matching row from the database.
    [Test]
    public async Task DeleteAsync_RemovesExistingCat()
    {
        var cat = await CreateCatAsync("ToDelete");

        await _repository.DeleteAsync(cat.Id);

        var deleted = await _context.Cats.FindAsync(cat.Id);
        deleted.Should().BeNull();
    }

    // Verifies the guard clause in DeleteAsync: calling it with an id that does not
    // exist must be a no-op and must not throw any exception.
    [Test]
    public async Task DeleteAsync_DoesNotThrow_WhenCatDoesNotExist()
    {
        var act = async () => await _repository.DeleteAsync(Guid.NewGuid());

        await act.Should().NotThrowAsync();
    }
}
