using CatTracker.Domain.Enums;

namespace CatTracker.Domain.Entities;

public class FeedingLog
{
    public Guid Id { get; private set; }
    public Guid CatId { get; private set; }
    public DateTime LoggedAt { get; private set; }
    public string FoodBrand { get; private set; } = string.Empty;
    public FoodType FoodType { get; private set; }
    public decimal AmountGrams { get; private set; }
    public string? Notes { get; private set; }

    // Private parameterless constructor for EF Core
    private FeedingLog() { }

    public static FeedingLog Create(
        Guid catId,
        string foodBrand,
        FoodType foodType,
        decimal amountGrams,
        string? notes = null,
        DateTime? loggedAt = null)
    {
        return new FeedingLog
        {
            Id = Guid.NewGuid(),
            CatId = catId,
            LoggedAt = loggedAt ?? DateTime.UtcNow,
            FoodBrand = foodBrand,
            FoodType = foodType,
            AmountGrams = amountGrams,
            Notes = notes,
        };
    }
}
