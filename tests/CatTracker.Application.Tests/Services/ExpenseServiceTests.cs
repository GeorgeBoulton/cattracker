using CatTracker.Application.DTOs;
using CatTracker.Application.Interfaces;
using CatTracker.Application.Services;
using CatTracker.Domain.Entities;
using CatTracker.Domain.Enums;
using CatTracker.Domain.Interfaces;

namespace CatTracker.Application.Tests.Services;

[TestFixture]
public class ExpenseServiceTests
{
    private IExpenseRepository _expenseRepository = null!;
    private IExpenseService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _expenseRepository = Substitute.For<IExpenseRepository>();
        _sut = new ExpenseService(_expenseRepository);
    }

    // CreateAsync must return an ExpenseResponse with all fields mapped from the request,
    // confirming the entity is constructed via the factory method and every property is
    // surfaced correctly through the mapping layer.
    [Test]
    public async Task CreateAsync_ValidRequest_ReturnsResponseWithAllFieldsMapped()
    {
        var catId = Guid.NewGuid();
        var date = new DateOnly(2025, 5, 10);
        var request = new CreateExpenseRequest(
            catId,
            ExpenseCategory.VetBill,
            150.00m,
            date,
            "Annual vaccination",
            "No issues",
            IsRecurring: true,
            RecurringIntervalDays: 365);

        var result = await _sut.CreateAsync(request);

        result.Should().NotBeNull();
        result.CatId.Should().Be(catId);
        result.Category.Should().Be(ExpenseCategory.VetBill);
        result.Amount.Should().Be(150.00m);
        result.Date.Should().Be(date);
        result.Description.Should().Be("Annual vaccination");
        result.Notes.Should().Be("No issues");
        result.IsRecurring.Should().BeTrue();
        result.RecurringIntervalDays.Should().Be(365);
        await _expenseRepository.Received(1).AddAsync(Arg.Any<Expense>());
    }

    // CreateAsync with IsRecurring=false must produce a response where RecurringIntervalDays
    // is null, because the domain entity strips interval data for non-recurring expenses.
    [Test]
    public async Task CreateAsync_NonRecurring_RecurringIntervalDaysIsNull()
    {
        var request = new CreateExpenseRequest(
            Guid.NewGuid(),
            ExpenseCategory.Food,
            30.00m,
            new DateOnly(2025, 6, 1),
            "Monthly food bag",
            null,
            IsRecurring: false,
            RecurringIntervalDays: 30);

        var result = await _sut.CreateAsync(request);

        result.IsRecurring.Should().BeFalse();
        result.RecurringIntervalDays.Should().BeNull();
    }

    // GetByDateRangeAsync must return all expenses within the range mapped to responses,
    // confirming list mapping works and the repository is called with the exact arguments.
    [Test]
    public async Task GetByDateRangeAsync_WithExpenses_ReturnsMappedResponses()
    {
        var catId = Guid.NewGuid();
        var from = new DateOnly(2025, 1, 1);
        var to = new DateOnly(2025, 3, 31);
        var expenses = new List<Expense>
        {
            Expense.Create(catId, ExpenseCategory.Food, 25.00m, new DateOnly(2025, 1, 15), "Dry food"),
            Expense.Create(catId, ExpenseCategory.Litter, 10.00m, new DateOnly(2025, 2, 10), "Litter bag"),
        };
        _expenseRepository.GetByDateRangeAsync(catId, from, to).Returns(expenses);

        var result = await _sut.GetByDateRangeAsync(catId, from, to);

        result.Should().HaveCount(2);
        result[0].Description.Should().Be("Dry food");
        result[0].Category.Should().Be(ExpenseCategory.Food);
        result[1].Description.Should().Be("Litter bag");
        result[1].Category.Should().Be(ExpenseCategory.Litter);
        await _expenseRepository.Received(1).GetByDateRangeAsync(catId, from, to);
    }

    // GetByDateRangeAsync when no expenses exist must return an empty list, confirming the
    // service handles an empty repository result without throwing or returning null.
    [Test]
    public async Task GetByDateRangeAsync_NoExpenses_ReturnsEmptyList()
    {
        var catId = Guid.NewGuid();
        var from = new DateOnly(2025, 1, 1);
        var to = new DateOnly(2025, 12, 31);
        _expenseRepository.GetByDateRangeAsync(catId, from, to).Returns(new List<Expense>());

        var result = await _sut.GetByDateRangeAsync(catId, from, to);

        result.Should().BeEmpty();
    }

    // GetByIdAsync must return the mapped response when the repository finds the expense,
    // confirming the non-null path maps every field correctly.
    [Test]
    public async Task GetByIdAsync_ExistingExpense_ReturnsMappedResponse()
    {
        var catId = Guid.NewGuid();
        var expense = Expense.Create(catId, ExpenseCategory.Insurance, 45.00m, new DateOnly(2025, 4, 1), "Monthly premium");
        _expenseRepository.GetByIdAsync(expense.Id).Returns(expense);

        var result = await _sut.GetByIdAsync(expense.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(expense.Id);
        result.CatId.Should().Be(catId);
        result.Category.Should().Be(ExpenseCategory.Insurance);
        result.Amount.Should().Be(45.00m);
        result.Description.Should().Be("Monthly premium");
    }

    // GetByIdAsync must return null when the repository returns null, verifying
    // that the null propagation path is handled gracefully without an exception.
    [Test]
    public async Task GetByIdAsync_NonExistingExpense_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _expenseRepository.GetByIdAsync(id).Returns((Expense?)null);

        var result = await _sut.GetByIdAsync(id);

        result.Should().BeNull();
    }

    // DeleteAsync must forward the exact id to the repository, confirming the service
    // does not swallow the call or substitute a different identifier.
    [Test]
    public async Task DeleteAsync_ValidId_CallsRepository()
    {
        var id = Guid.NewGuid();

        await _sut.DeleteAsync(id);

        await _expenseRepository.Received(1).DeleteAsync(id);
    }

    // GetMonthlyTotalsAsync must group expenses by year, month, and category and sum
    // amounts within each group, producing one MonthlyExpenseSummary per unique combination.
    [Test]
    public async Task GetMonthlyTotalsAsync_MultipleExpenses_GroupsAndSumsByYearMonthCategory()
    {
        var catId = Guid.NewGuid();
        var from = new DateOnly(2025, 1, 1);
        var to = new DateOnly(2025, 2, 28);
        var expenses = new List<Expense>
        {
            Expense.Create(catId, ExpenseCategory.Food, 20.00m, new DateOnly(2025, 1, 5), "Food week 1"),
            Expense.Create(catId, ExpenseCategory.Food, 18.00m, new DateOnly(2025, 1, 20), "Food week 3"),
            Expense.Create(catId, ExpenseCategory.Litter, 12.00m, new DateOnly(2025, 1, 10), "Litter January"),
            Expense.Create(catId, ExpenseCategory.Food, 22.00m, new DateOnly(2025, 2, 7), "Food February"),
        };
        _expenseRepository.GetByDateRangeAsync(catId, from, to).Returns(expenses);

        var result = await _sut.GetMonthlyTotalsAsync(catId, from, to);

        result.Should().HaveCount(3);

        var janFood = result.First(s => s.Year == 2025 && s.Month == 1 && s.Category == ExpenseCategory.Food);
        janFood.Total.Should().Be(38.00m);

        var janLitter = result.First(s => s.Year == 2025 && s.Month == 1 && s.Category == ExpenseCategory.Litter);
        janLitter.Total.Should().Be(12.00m);

        var febFood = result.First(s => s.Year == 2025 && s.Month == 2 && s.Category == ExpenseCategory.Food);
        febFood.Total.Should().Be(22.00m);
    }

    // GetMonthlyTotalsAsync with a single expense must return exactly one summary record,
    // confirming the grouping logic works correctly for a trivial single-item case.
    [Test]
    public async Task GetMonthlyTotalsAsync_SingleExpense_ReturnsSingleSummary()
    {
        var catId = Guid.NewGuid();
        var from = new DateOnly(2025, 6, 1);
        var to = new DateOnly(2025, 6, 30);
        var expenses = new List<Expense>
        {
            Expense.Create(catId, ExpenseCategory.VetBill, 200.00m, new DateOnly(2025, 6, 15), "Check-up"),
        };
        _expenseRepository.GetByDateRangeAsync(catId, from, to).Returns(expenses);

        var result = await _sut.GetMonthlyTotalsAsync(catId, from, to);

        result.Should().HaveCount(1);
        result[0].Year.Should().Be(2025);
        result[0].Month.Should().Be(6);
        result[0].Category.Should().Be(ExpenseCategory.VetBill);
        result[0].Total.Should().Be(200.00m);
    }

    // GetMonthlyTotalsAsync with no expenses must return an empty list, confirming the
    // grouping and summing logic does not fail or return null for an empty input.
    [Test]
    public async Task GetMonthlyTotalsAsync_NoExpenses_ReturnsEmptyList()
    {
        var catId = Guid.NewGuid();
        var from = new DateOnly(2025, 1, 1);
        var to = new DateOnly(2025, 12, 31);
        _expenseRepository.GetByDateRangeAsync(catId, from, to).Returns(new List<Expense>());

        var result = await _sut.GetMonthlyTotalsAsync(catId, from, to);

        result.Should().BeEmpty();
    }

    // GetMonthlyTotalsAsync must return summaries ordered by year, then month, then category,
    // so callers can rely on a consistent ordering without needing to sort themselves.
    [Test]
    public async Task GetMonthlyTotalsAsync_MultipleMonths_ReturnsResultsOrderedByYearMonthCategory()
    {
        var catId = Guid.NewGuid();
        var from = new DateOnly(2025, 1, 1);
        var to = new DateOnly(2025, 3, 31);
        var expenses = new List<Expense>
        {
            Expense.Create(catId, ExpenseCategory.Other, 5.00m, new DateOnly(2025, 3, 1), "March other"),
            Expense.Create(catId, ExpenseCategory.Food, 15.00m, new DateOnly(2025, 2, 1), "Feb food"),
            Expense.Create(catId, ExpenseCategory.Food, 10.00m, new DateOnly(2025, 1, 1), "Jan food"),
        };
        _expenseRepository.GetByDateRangeAsync(catId, from, to).Returns(expenses);

        var result = await _sut.GetMonthlyTotalsAsync(catId, from, to);

        result.Should().HaveCount(3);
        result[0].Month.Should().Be(1);
        result[1].Month.Should().Be(2);
        result[2].Month.Should().Be(3);
    }
}
