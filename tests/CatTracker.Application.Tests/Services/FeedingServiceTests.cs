using CatTracker.Application.DTOs;
using CatTracker.Application.Interfaces;
using CatTracker.Application.Services;
using CatTracker.Domain.Entities;
using CatTracker.Domain.Enums;
using CatTracker.Domain.Interfaces;
using CatTracker.Domain.Services;

namespace CatTracker.Application.Tests.Services;

[TestFixture]
public class FeedingServiceTests
{
    private IFeedingLogRepository _feedingLogRepository = null!;
    private IFoodStockRepository _foodStockRepository = null!;
    private FoodStockService _foodStockService = null!;
    private IFeedingService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _feedingLogRepository = Substitute.For<IFeedingLogRepository>();
        _foodStockRepository = Substitute.For<IFoodStockRepository>();
        _foodStockService = new FoodStockService();
        _sut = new FeedingService(_feedingLogRepository, _foodStockRepository, _foodStockService);
    }

    // LogFeedingAsync must return a FeedingLogResponse with fields mapped from the request,
    // confirming the entity is created and the mapping is correct.
    [Test]
    public async Task LogFeedingAsync_ValidRequest_ReturnsExpectedResponse()
    {
        var catId = Guid.NewGuid();
        var loggedAt = new DateTime(2025, 6, 1, 8, 0, 0, DateTimeKind.Utc);
        var request = new LogFeedingRequest(catId, "RoyalCanin", FoodType.Dry, 100m, "Morning feed", loggedAt);

        FeedingLog? capturedLog = null;
        await _feedingLogRepository.AddAsync(Arg.Do<FeedingLog>(l => capturedLog = l));

        var result = await _sut.LogFeedingAsync(request);

        result.Should().NotBeNull();
        result.CatId.Should().Be(catId);
        result.FoodBrand.Should().Be("RoyalCanin");
        result.FoodType.Should().Be(FoodType.Dry);
        result.AmountGrams.Should().Be(100m);
        result.Notes.Should().Be("Morning feed");
        result.LoggedAt.Should().Be(loggedAt);
        await _feedingLogRepository.Received(1).AddAsync(Arg.Any<FeedingLog>());
    }

    // GetRecentFeedingsAsync must return all feeding logs mapped to responses, confirming
    // list mapping works correctly and preserves all entries.
    [Test]
    public async Task GetRecentFeedingsAsync_ReturnsMappedList()
    {
        var catId = Guid.NewGuid();
        var logs = new List<FeedingLog>
        {
            FeedingLog.Create(catId, "BrandA", FoodType.Wet, 80m, null, new DateTime(2025, 6, 1, 8, 0, 0, DateTimeKind.Utc)),
            FeedingLog.Create(catId, "BrandB", FoodType.Dry, 60m, "notes", new DateTime(2025, 6, 1, 18, 0, 0, DateTimeKind.Utc)),
        };
        _feedingLogRepository.GetRecentAsync(catId, 20).Returns(logs);

        var result = await _sut.GetRecentFeedingsAsync(catId);

        result.Should().HaveCount(2);
        result[0].FoodBrand.Should().Be("BrandA");
        result[1].FoodBrand.Should().Be("BrandB");
    }

    // GetLatestFeedingAsync must return null when the repository returns null, verifying
    // that the null propagation path is handled without throwing.
    [Test]
    public async Task GetLatestFeedingAsync_ReturnsNull_WhenNoFeedings()
    {
        var catId = Guid.NewGuid();
        _feedingLogRepository.GetLatestAsync(catId).Returns((FeedingLog?)null);

        var result = await _sut.GetLatestFeedingAsync(catId);

        result.Should().BeNull();
    }

    // CreateFoodStockAsync must return a FoodStockResponse with DaysRemaining and IsLowStock
    // computed by FoodStockService, confirming domain service integration in the mapping.
    [Test]
    public async Task CreateFoodStockAsync_ValidRequest_ReturnsMappedResponse()
    {
        var catId = Guid.NewGuid();
        var request = new CreateFoodStockRequest(catId, "Hills", FoodType.Dry, 700m, 100m, 7);

        var result = await _sut.CreateFoodStockAsync(request);

        result.Should().NotBeNull();
        result.CatId.Should().Be(catId);
        result.FoodBrand.Should().Be("Hills");
        result.FoodType.Should().Be(FoodType.Dry);
        result.QuantityGrams.Should().Be(700m);
        result.DailyUsageGrams.Should().Be(100m);
        result.LowStockThresholdDays.Should().Be(7);
        result.DaysRemaining.Should().Be(7m);   // 700 / 100 = 7
        result.IsLowStock.Should().BeTrue();     // 7 <= 7
        await _foodStockRepository.Received(1).AddAsync(Arg.Any<FoodStock>());
    }

    // UpdateFoodStockAsync must throw KeyNotFoundException when no stock exists for the
    // given id, so callers can distinguish "not found" from other failure modes.
    [Test]
    public async Task UpdateFoodStockAsync_ThrowsKeyNotFoundException_WhenNotFound()
    {
        var missingId = Guid.NewGuid();
        _foodStockRepository.GetByIdAsync(missingId).Returns((FoodStock?)null);

        var request = new UpdateFoodStockRequest(500m, 80m, 5);

        Func<Task> act = () => _sut.UpdateFoodStockAsync(missingId, request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // GetBrandSuggestionsAsync must return the list of distinct brands provided by the
    // repository, confirming the autocomplete contract passes data through without mutation.
    [Test]
    public async Task GetBrandSuggestionsAsync_ReturnsList()
    {
        var catId = Guid.NewGuid();
        var brands = new List<string> { "RoyalCanin", "Hills", "Purina" };
        _feedingLogRepository.GetDistinctBrandsAsync(catId).Returns(brands);

        var result = await _sut.GetBrandSuggestionsAsync(catId);

        result.Should().HaveCount(3);
        result.Should().Contain("RoyalCanin");
        result.Should().Contain("Hills");
        result.Should().Contain("Purina");
    }
}
