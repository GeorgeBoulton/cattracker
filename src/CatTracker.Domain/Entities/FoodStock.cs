using CatTracker.Domain.Enums;

namespace CatTracker.Domain.Entities;

public class FoodStock
{
    public Guid Id { get; private set; }
    public Guid CatId { get; private set; }
    public string FoodBrand { get; private set; } = string.Empty;
    public FoodType FoodType { get; private set; }
    public decimal QuantityGrams { get; private set; }
    public decimal DailyUsageGrams { get; private set; }
    public int LowStockThresholdDays { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Private parameterless constructor for EF Core
    private FoodStock() { }

    public static FoodStock Create(
        Guid catId,
        string foodBrand,
        FoodType foodType,
        decimal quantityGrams,
        decimal dailyUsageGrams,
        int lowStockThresholdDays = 7)
    {
        return new FoodStock
        {
            Id = Guid.NewGuid(),
            CatId = catId,
            FoodBrand = foodBrand,
            FoodType = foodType,
            QuantityGrams = quantityGrams,
            DailyUsageGrams = dailyUsageGrams,
            LowStockThresholdDays = lowStockThresholdDays,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateStock(decimal quantityGrams)
    {
        QuantityGrams = quantityGrams;
        UpdatedAt = DateTime.UtcNow;
    }
}
