using CatTracker.Domain.Enums;

namespace CatTracker.Application.DTOs;

public record MonthlyExpenseSummary(
    int Year,
    int Month,
    ExpenseCategory Category,
    decimal Total
);
