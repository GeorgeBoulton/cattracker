using CatTracker.Domain.Entities;
using CatTracker.Domain.Enums;
using CatTracker.Infrastructure.Data;
using CatTracker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace CatTracker.Infrastructure.Tests.Repositories;

[TestFixture]
public class ExpenseRepositoryTests
{
    private PostgreSqlContainer _dbContainer = null!;
    private CatTrackerDbContext _context = null!;
    private ExpenseRepository _repository = null!;

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

        _repository = new ExpenseRepository(_context);
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
        _context.Expenses.RemoveRange(_context.Expenses);
        _context.Cats.RemoveRange(_context.Cats);
        await _context.SaveChangesAsync();
    }

    // Helper: inserts a Cat row required by the Expense foreign key constraint
    private async Task<Cat> CreateCatAsync(string name = "Whiskers")
    {
        var cat = Cat.Create(name);
        await _context.Cats.AddAsync(cat);
        await _context.SaveChangesAsync();
        return cat;
    }

    // Verifies that GetByDateRangeAsync returns expenses whose Date falls within the
    // inclusive [from, to] range, confirming both boundary dates are included.
    [Test]
    public async Task GetByDateRangeAsync_ReturnsExpensesInRange()
    {
        var cat = await CreateCatAsync();
        var from = new DateOnly(2026, 1, 1);
        var to = new DateOnly(2026, 1, 31);

        var onFrom = Expense.Create(cat.Id, ExpenseCategory.Food, 20.00m, from, "On from boundary");
        var inMiddle = Expense.Create(cat.Id, ExpenseCategory.Litter, 15.00m, new DateOnly(2026, 1, 15), "In range");
        var onTo = Expense.Create(cat.Id, ExpenseCategory.VetBill, 100.00m, to, "On to boundary");

        await _context.Expenses.AddRangeAsync(onFrom, inMiddle, onTo);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByDateRangeAsync(cat.Id, from, to);

        result.Should().HaveCount(3);
        result.Should().Contain(e => e.Id == onFrom.Id);
        result.Should().Contain(e => e.Id == inMiddle.Id);
        result.Should().Contain(e => e.Id == onTo.Id);
    }

    // Verifies that expenses whose Date falls strictly outside [from, to] are excluded,
    // so the boundary comparison uses >= and <= rather than > and <.
    [Test]
    public async Task GetByDateRangeAsync_ExcludesExpensesOutsideRange()
    {
        var cat = await CreateCatAsync();
        var from = new DateOnly(2026, 2, 1);
        var to = new DateOnly(2026, 2, 28);

        var before = Expense.Create(cat.Id, ExpenseCategory.Food, 20.00m, new DateOnly(2026, 1, 31), "Before range");
        var inRange = Expense.Create(cat.Id, ExpenseCategory.Food, 25.00m, new DateOnly(2026, 2, 15), "In range");
        var after = Expense.Create(cat.Id, ExpenseCategory.Food, 30.00m, new DateOnly(2026, 3, 1), "After range");

        await _context.Expenses.AddRangeAsync(before, inRange, after);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByDateRangeAsync(cat.Id, from, to);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(inRange.Id);
    }

    // Verifies that the CatId filter is applied so expenses belonging to a different cat
    // are never returned even if their dates fall within the requested range.
    [Test]
    public async Task GetByDateRangeAsync_ReturnsOnlyExpensesForSpecifiedCat()
    {
        var cat = await CreateCatAsync();
        var otherCat = await CreateCatAsync("OtherCat");
        var from = new DateOnly(2026, 1, 1);
        var to = new DateOnly(2026, 1, 31);

        var expenseForCat = Expense.Create(cat.Id, ExpenseCategory.Food, 20.00m, new DateOnly(2026, 1, 10), "Cat food");
        var expenseForOther = Expense.Create(otherCat.Id, ExpenseCategory.Litter, 15.00m, new DateOnly(2026, 1, 10), "Other cat litter");

        await _context.Expenses.AddRangeAsync(expenseForCat, expenseForOther);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByDateRangeAsync(cat.Id, from, to);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(expenseForCat.Id);
    }

    // Verifies that an empty list is returned (not null, not an exception)
    // when no expenses exist for the given cat within the date range.
    [Test]
    public async Task GetByDateRangeAsync_ReturnsEmptyList_WhenNoMatches()
    {
        var cat = await CreateCatAsync();

        var result = await _repository.GetByDateRangeAsync(cat.Id, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));

        result.Should().BeEmpty();
    }

    // Verifies that expenses are ordered by Date descending so the most recent
    // expense appears first in the returned list.
    [Test]
    public async Task GetByDateRangeAsync_ReturnsExpensesOrderedByDateDescending()
    {
        var cat = await CreateCatAsync();
        var from = new DateOnly(2026, 1, 1);
        var to = new DateOnly(2026, 1, 31);

        var oldest = Expense.Create(cat.Id, ExpenseCategory.Food, 10.00m, new DateOnly(2026, 1, 1), "Oldest");
        var middle = Expense.Create(cat.Id, ExpenseCategory.Litter, 12.00m, new DateOnly(2026, 1, 15), "Middle");
        var newest = Expense.Create(cat.Id, ExpenseCategory.VetBill, 50.00m, new DateOnly(2026, 1, 30), "Newest");

        await _context.Expenses.AddRangeAsync(oldest, middle, newest);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByDateRangeAsync(cat.Id, from, to);

        result.Should().HaveCount(3);
        result[0].Id.Should().Be(newest.Id);
        result[1].Id.Should().Be(middle.Id);
        result[2].Id.Should().Be(oldest.Id);
    }

    // Verifies that an expense can be retrieved by its primary key with all fields intact.
    [Test]
    public async Task GetByIdAsync_ReturnsExpense_WhenExists()
    {
        var cat = await CreateCatAsync();
        var expense = Expense.Create(
            cat.Id,
            ExpenseCategory.VetBill,
            150.00m,
            new DateOnly(2026, 1, 10),
            "Annual checkup bill",
            notes: "Includes vaccinations");

        await _context.Expenses.AddAsync(expense);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(expense.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(expense.Id);
        result.Description.Should().Be("Annual checkup bill");
        result.Amount.Should().Be(150.00m);
    }

    // Verifies that null is returned for an unknown id rather than throwing an exception.
    [Test]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    // Verifies that AddAsync actually writes the expense entity to the database
    // so it survives a fresh query with all fields persisted correctly.
    [Test]
    public async Task AddAsync_PersistsExpenseToDatabase()
    {
        var cat = await CreateCatAsync();
        var expense = Expense.Create(
            cat.Id,
            ExpenseCategory.Insurance,
            30.00m,
            new DateOnly(2026, 1, 20),
            "Monthly pet insurance",
            notes: "Direct debit",
            isRecurring: true,
            recurringIntervalDays: 30);

        await _repository.AddAsync(expense);

        var persisted = await _context.Expenses.FindAsync(expense.Id);
        persisted.Should().NotBeNull();
        persisted!.Description.Should().Be("Monthly pet insurance");
        persisted.Amount.Should().Be(30.00m);
        persisted.Category.Should().Be(ExpenseCategory.Insurance);
        persisted.IsRecurring.Should().BeTrue();
        persisted.RecurringIntervalDays.Should().Be(30);
    }

    // Verifies that DeleteAsync actually removes the row from the database so a
    // subsequent lookup by the same id returns null.
    [Test]
    public async Task DeleteAsync_RemovesExpense()
    {
        var cat = await CreateCatAsync();
        var expense = Expense.Create(cat.Id, ExpenseCategory.Food, 18.00m, new DateOnly(2026, 1, 5), "Cat food bag");
        await _context.Expenses.AddAsync(expense);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(expense.Id);

        var deleted = await _context.Expenses.FindAsync(expense.Id);
        deleted.Should().BeNull();
    }

    // Verifies the guard clause in DeleteAsync: calling it with an id that does not
    // exist should be a no-op and must not throw an exception.
    [Test]
    public async Task DeleteAsync_DoesNotThrow_WhenExpenseDoesNotExist()
    {
        var act = async () => await _repository.DeleteAsync(Guid.NewGuid());

        await act.Should().NotThrowAsync();
    }
}
