using CatTracker.Domain.Entities;

namespace CatTracker.Domain.Services;

public class FoodStockService
{
    public decimal DaysRemaining(FoodStock stock)
    {
        if (stock.DailyUsageGrams == 0)
            return decimal.MaxValue;

        return stock.QuantityGrams / stock.DailyUsageGrams;
    }

    public bool IsLowStock(FoodStock stock)
    {
        return DaysRemaining(stock) <= stock.LowStockThresholdDays;
    }
}
