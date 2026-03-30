using CatTracker.Domain.Entities;
using CatTracker.Domain.Enums;
using CatTracker.Domain.Services;

namespace CatTracker.Domain.Tests.Services;

[TestFixture]
public class FoodStockServiceTests
{
    private FoodStockService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new FoodStockService();
    }

    // DaysRemaining should divide quantity by daily usage to yield the number of days of food left.
    [Test]
    public void DaysRemaining_NormalUsage_ReturnsCorrectDays()
    {
        var stock = FoodStock.Create(Guid.NewGuid(), "Whiskas", FoodType.Dry, 700m, 100m);

        var result = _sut.DaysRemaining(stock);

        result.Should().Be(7m);
    }

    // When DailyUsageGrams is 0 dividing by zero is undefined; the service should signal
    // "infinite" supply by returning decimal.MaxValue rather than throwing.
    [Test]
    public void DaysRemaining_ZeroDailyUsage_ReturnsDecimalMaxValue()
    {
        var stock = FoodStock.Create(Guid.NewGuid(), "Whiskas", FoodType.Dry, 700m, 0m);

        var result = _sut.DaysRemaining(stock);

        result.Should().Be(decimal.MaxValue);
    }

    // Partial-day results must be preserved; integer truncation would give incorrect data.
    [Test]
    public void DaysRemaining_PartialDays_ReturnsDecimalResult()
    {
        var stock = FoodStock.Create(Guid.NewGuid(), "Royal Canin", FoodType.Wet, 150m, 100m);

        var result = _sut.DaysRemaining(stock);

        result.Should().Be(1.5m);
    }

    // IsLowStock must alert when the remaining days are strictly below the threshold,
    // ensuring the owner has time to reorder before running out.
    [Test]
    public void IsLowStock_DaysRemainingBelowThreshold_ReturnsTrue()
    {
        // 300g / 100g/day = 3 days remaining; threshold is 7 days → low stock
        var stock = FoodStock.Create(Guid.NewGuid(), "Hills", FoodType.Dry, 300m, 100m, lowStockThresholdDays: 7);

        var result = _sut.IsLowStock(stock);

        result.Should().BeTrue();
    }

    // IsLowStock must not trigger a false alarm when there is plenty of food left.
    [Test]
    public void IsLowStock_DaysRemainingAboveThreshold_ReturnsFalse()
    {
        // 1400g / 100g/day = 14 days remaining; threshold is 7 days → not low stock
        var stock = FoodStock.Create(Guid.NewGuid(), "Hills", FoodType.Dry, 1400m, 100m, lowStockThresholdDays: 7);

        var result = _sut.IsLowStock(stock);

        result.Should().BeFalse();
    }

    // When daily usage is 0 the supply is effectively infinite (decimal.MaxValue days),
    // so IsLowStock should never report low stock regardless of the threshold.
    [Test]
    public void IsLowStock_ZeroDailyUsage_ReturnsFalse()
    {
        var stock = FoodStock.Create(Guid.NewGuid(), "Purina", FoodType.Treat, 100m, 0m, lowStockThresholdDays: 7);

        var result = _sut.IsLowStock(stock);

        result.Should().BeFalse();
    }

    // The boundary condition (days remaining == threshold) must return true because the
    // contract uses <=: being exactly at the threshold is considered low stock.
    [Test]
    public void IsLowStock_DaysRemainingExactlyAtThreshold_ReturnsTrue()
    {
        // 700g / 100g/day = 7 days remaining; threshold is 7 days → exactly at boundary
        var stock = FoodStock.Create(Guid.NewGuid(), "Iams", FoodType.Mixed, 700m, 100m, lowStockThresholdDays: 7);

        var result = _sut.IsLowStock(stock);

        result.Should().BeTrue();
    }
}
