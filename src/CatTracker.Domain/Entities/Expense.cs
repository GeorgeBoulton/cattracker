using CatTracker.Domain.Enums;

namespace CatTracker.Domain.Entities;

public class Expense
{
    public Guid Id { get; private set; }
    public Guid CatId { get; private set; }
    public ExpenseCategory Category { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly Date { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public bool IsRecurring { get; private set; }
    public int? RecurringIntervalDays { get; private set; }

    // Private parameterless constructor for EF Core
    private Expense() { }

    public static Expense Create(
        Guid catId,
        ExpenseCategory category,
        decimal amount,
        DateOnly date,
        string description,
        string? notes = null,
        bool isRecurring = false,
        int? recurringIntervalDays = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        if (description.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters.", nameof(description));

        return new Expense
        {
            Id = Guid.NewGuid(),
            CatId = catId,
            Category = category,
            Amount = amount,
            Date = date,
            Description = description,
            Notes = notes,
            IsRecurring = isRecurring,
            RecurringIntervalDays = isRecurring ? recurringIntervalDays : null,
        };
    }
}
