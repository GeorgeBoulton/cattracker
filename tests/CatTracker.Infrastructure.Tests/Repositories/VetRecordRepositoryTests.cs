using CatTracker.Domain.Entities;
using CatTracker.Domain.Enums;
using CatTracker.Infrastructure.Data;
using CatTracker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace CatTracker.Infrastructure.Tests.Repositories;

[TestFixture]
public class VetRecordRepositoryTests
{
    private PostgreSqlContainer _dbContainer = null!;
    private CatTrackerDbContext _context = null!;
    private VetRecordRepository _repository = null!;

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

        _repository = new VetRecordRepository(_context);
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
        _context.VetRecords.RemoveRange(_context.VetRecords);
        _context.Cats.RemoveRange(_context.Cats);
        await _context.SaveChangesAsync();
    }

    // Helper: inserts a Cat row required by the VetRecord foreign key constraint
    private async Task<Cat> CreateCatAsync(string name = "Whiskers")
    {
        var cat = Cat.Create(name);
        await _context.Cats.AddAsync(cat);
        await _context.SaveChangesAsync();
        return cat;
    }

    // Verifies that GetByCatAsync returns only records belonging to the requested cat,
    // so the CatId filter is actually applied and other cats' records are excluded.
    [Test]
    public async Task GetByCatAsync_ReturnsRecordsForCat()
    {
        var cat = await CreateCatAsync();
        var otherCat = await CreateCatAsync("OtherCat");

        var recordForCat = VetRecord.Create(cat.Id, VetRecordType.Visit, new DateOnly(2026, 1, 10), "Annual checkup");
        var recordForOther = VetRecord.Create(otherCat.Id, VetRecordType.Vaccination, new DateOnly(2026, 1, 5), "Rabies shot");

        await _context.VetRecords.AddRangeAsync(recordForCat, recordForOther);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByCatAsync(cat.Id);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(recordForCat.Id);
    }

    // Verifies that GetByCatAsync returns records ordered by Date descending,
    // so the most recent visit appears first in the list.
    [Test]
    public async Task GetByCatAsync_ReturnsRecordsOrderedByDateDescending()
    {
        var cat = await CreateCatAsync();

        var oldest = VetRecord.Create(cat.Id, VetRecordType.Visit, new DateOnly(2026, 1, 1), "First visit");
        var middle = VetRecord.Create(cat.Id, VetRecordType.Vaccination, new DateOnly(2026, 1, 5), "Vaccination");
        var newest = VetRecord.Create(cat.Id, VetRecordType.WeighIn, new DateOnly(2026, 1, 10), "Weigh-in");

        await _context.VetRecords.AddRangeAsync(oldest, middle, newest);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByCatAsync(cat.Id);

        result.Should().HaveCount(3);
        result[0].Id.Should().Be(newest.Id);
        result[1].Id.Should().Be(middle.Id);
        result[2].Id.Should().Be(oldest.Id);
    }

    // Verifies that an empty list is returned (not null, not an exception)
    // when no vet records exist for the given cat.
    [Test]
    public async Task GetByCatAsync_ReturnsEmpty_WhenNoRecordsExist()
    {
        var cat = await CreateCatAsync();

        var result = await _repository.GetByCatAsync(cat.Id);

        result.Should().BeEmpty();
    }

    // Verifies that GetUpcomingAsync returns records whose NextDueDate is on or before
    // the given upper-bound date, confirming the <= boundary is inclusive.
    [Test]
    public async Task GetUpcomingAsync_ReturnsRecordsWithNextDueDateOnOrBeforeUpperBound()
    {
        var cat = await CreateCatAsync();
        var upperBound = new DateOnly(2026, 2, 28);

        var dueBefore = VetRecord.Create(cat.Id, VetRecordType.Vaccination, new DateOnly(2026, 1, 1), "Rabies", nextDueDate: new DateOnly(2026, 2, 15));
        var dueOnBoundary = VetRecord.Create(cat.Id, VetRecordType.Vaccination, new DateOnly(2026, 1, 1), "FVRCP", nextDueDate: upperBound);
        var dueAfter = VetRecord.Create(cat.Id, VetRecordType.Vaccination, new DateOnly(2026, 1, 1), "Bordetella", nextDueDate: new DateOnly(2026, 3, 15));

        await _context.VetRecords.AddRangeAsync(dueBefore, dueOnBoundary, dueAfter);
        await _context.SaveChangesAsync();

        var result = await _repository.GetUpcomingAsync(cat.Id, upperBound);

        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Id == dueBefore.Id);
        result.Should().Contain(r => r.Id == dueOnBoundary.Id);
    }

    // Verifies that records without a NextDueDate are excluded from upcoming results,
    // so the null check in the filter is applied correctly.
    [Test]
    public async Task GetUpcomingAsync_ExcludesRecordsWithNoNextDueDate()
    {
        var cat = await CreateCatAsync();
        var upperBound = new DateOnly(2026, 12, 31);

        var withDueDate = VetRecord.Create(cat.Id, VetRecordType.Visit, new DateOnly(2026, 1, 1), "Annual visit", nextDueDate: new DateOnly(2026, 6, 1));
        var withoutDueDate = VetRecord.Create(cat.Id, VetRecordType.Procedure, new DateOnly(2026, 1, 1), "One-off procedure");

        await _context.VetRecords.AddRangeAsync(withDueDate, withoutDueDate);
        await _context.SaveChangesAsync();

        var result = await _repository.GetUpcomingAsync(cat.Id, upperBound);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(withDueDate.Id);
    }

    // Verifies that GetUpcomingAsync returns records ordered by NextDueDate ascending,
    // so the most imminent due date appears first.
    [Test]
    public async Task GetUpcomingAsync_ReturnsRecordsOrderedByNextDueDateAscending()
    {
        var cat = await CreateCatAsync();
        var upperBound = new DateOnly(2026, 12, 31);

        var dueLatest = VetRecord.Create(cat.Id, VetRecordType.Vaccination, new DateOnly(2026, 1, 1), "Rabies", nextDueDate: new DateOnly(2026, 10, 1));
        var dueEarliest = VetRecord.Create(cat.Id, VetRecordType.Vaccination, new DateOnly(2026, 1, 1), "FVRCP", nextDueDate: new DateOnly(2026, 3, 1));
        var dueMiddle = VetRecord.Create(cat.Id, VetRecordType.Vaccination, new DateOnly(2026, 1, 1), "Bordetella", nextDueDate: new DateOnly(2026, 6, 1));

        await _context.VetRecords.AddRangeAsync(dueLatest, dueEarliest, dueMiddle);
        await _context.SaveChangesAsync();

        var result = await _repository.GetUpcomingAsync(cat.Id, upperBound);

        result.Should().HaveCount(3);
        result[0].Id.Should().Be(dueEarliest.Id);
        result[1].Id.Should().Be(dueMiddle.Id);
        result[2].Id.Should().Be(dueLatest.Id);
    }

    // Verifies that a record can be retrieved by its primary key.
    [Test]
    public async Task GetByIdAsync_ReturnsRecord_WhenExists()
    {
        var cat = await CreateCatAsync();
        var record = VetRecord.Create(cat.Id, VetRecordType.Visit, new DateOnly(2026, 1, 10), "Annual checkup");
        await _context.VetRecords.AddAsync(record);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(record.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(record.Id);
    }

    // Verifies that null is returned for an unknown id rather than throwing an exception.
    [Test]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    // Verifies that AddAsync actually writes the entity to the database,
    // so it survives a fresh query with all fields intact.
    [Test]
    public async Task AddAsync_PersistsVetRecord()
    {
        var cat = await CreateCatAsync();
        var record = VetRecord.Create(
            cat.Id,
            VetRecordType.Vaccination,
            new DateOnly(2026, 1, 15),
            "Annual rabies vaccination",
            clinicName: "City Vet Clinic",
            vetName: "Dr. Smith",
            cost: 75.00m);

        await _repository.AddAsync(record);

        var persisted = await _context.VetRecords.FindAsync(record.Id);
        persisted.Should().NotBeNull();
        persisted!.Description.Should().Be("Annual rabies vaccination");
        persisted.ClinicName.Should().Be("City Vet Clinic");
        persisted.Cost.Should().Be(75.00m);
    }

    // Verifies that UpdateAsync flushes the tracked entity state to the database,
    // so a subsequent read after detaching reflects the persisted record.
    [Test]
    public async Task UpdateAsync_PersistsChanges()
    {
        var cat = await CreateCatAsync();
        var record = VetRecord.Create(cat.Id, VetRecordType.Visit, new DateOnly(2026, 1, 10), "Initial visit");
        await _context.VetRecords.AddAsync(record);
        await _context.SaveChangesAsync();

        // Call UpdateAsync on the tracked entity to verify it saves without error
        await _repository.UpdateAsync(record);

        // Detach to force a real DB read
        _context.Entry(record).State = EntityState.Detached;
        var updated = await _context.VetRecords.FindAsync(record.Id);
        updated.Should().NotBeNull();
        updated!.Description.Should().Be("Initial visit");
    }

    // Verifies that DeleteAsync actually removes the row from the database.
    [Test]
    public async Task DeleteAsync_RemovesExistingRecord()
    {
        var cat = await CreateCatAsync();
        var record = VetRecord.Create(cat.Id, VetRecordType.Visit, new DateOnly(2026, 1, 10), "Checkup");
        await _context.VetRecords.AddAsync(record);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(record.Id);

        var deleted = await _context.VetRecords.FindAsync(record.Id);
        deleted.Should().BeNull();
    }

    // Verifies the guard clause in DeleteAsync: calling it with an id that
    // does not exist should be a no-op and must not throw an exception.
    [Test]
    public async Task DeleteAsync_DoesNotThrow_WhenRecordDoesNotExist()
    {
        var act = async () => await _repository.DeleteAsync(Guid.NewGuid());

        await act.Should().NotThrowAsync();
    }
}
