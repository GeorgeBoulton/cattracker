using CatTracker.Application.DTOs;

namespace CatTracker.Application.Interfaces;

public interface IFeedingService
{
    Task<FeedingLogResponse> LogFeedingAsync(LogFeedingRequest request);
    Task<IReadOnlyList<FeedingLogResponse>> GetRecentFeedingsAsync(Guid catId, int count = 20);
    Task<FeedingLogResponse?> GetLatestFeedingAsync(Guid catId);
    Task DeleteFeedingAsync(Guid id);

    Task<IReadOnlyList<FoodStockResponse>> GetFoodStockAsync(Guid catId);
    Task<FoodStockResponse> CreateFoodStockAsync(CreateFoodStockRequest request);
    Task UpdateFoodStockAsync(Guid id, UpdateFoodStockRequest request);
    Task DeleteFoodStockAsync(Guid id);

    Task<IReadOnlyList<string>> GetBrandSuggestionsAsync(Guid catId);
}
