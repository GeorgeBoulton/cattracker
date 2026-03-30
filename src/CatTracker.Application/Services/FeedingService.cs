using CatTracker.Application.DTOs;
using CatTracker.Application.Interfaces;
using CatTracker.Domain.Entities;
using CatTracker.Domain.Interfaces;
using CatTracker.Domain.Services;

namespace CatTracker.Application.Services;

public class FeedingService : IFeedingService
{
    private readonly IFeedingLogRepository _feedingLogRepository;
    private readonly IFoodStockRepository _foodStockRepository;
    private readonly FoodStockService _foodStockService;

    public FeedingService(
        IFeedingLogRepository feedingLogRepository,
        IFoodStockRepository foodStockRepository,
        FoodStockService foodStockService)
    {
        _feedingLogRepository = feedingLogRepository;
        _foodStockRepository = foodStockRepository;
        _foodStockService = foodStockService;
    }

    public async Task<FeedingLogResponse> LogFeedingAsync(LogFeedingRequest request)
    {
        var log = FeedingLog.Create(
            request.CatId,
            request.FoodBrand,
            request.FoodType,
            request.AmountGrams,
            request.Notes,
            request.LoggedAt);

        await _feedingLogRepository.AddAsync(log);

        return MapToResponse(log);
    }

    public async Task<IReadOnlyList<FeedingLogResponse>> GetRecentFeedingsAsync(Guid catId, int count = 20)
    {
        var logs = await _feedingLogRepository.GetRecentAsync(catId, count);

        return logs
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<FeedingLogResponse?> GetLatestFeedingAsync(Guid catId)
    {
        var log = await _feedingLogRepository.GetLatestAsync(catId);
        if (log is null)
            return null;

        return MapToResponse(log);
    }

    public async Task DeleteFeedingAsync(Guid id)
    {
        await _feedingLogRepository.DeleteAsync(id);
    }

    public async Task<IReadOnlyList<FoodStockResponse>> GetFoodStockAsync(Guid catId)
    {
        var stocks = await _foodStockRepository.GetByCatAsync(catId);

        return stocks
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<FoodStockResponse> CreateFoodStockAsync(CreateFoodStockRequest request)
    {
        var stock = FoodStock.Create(
            request.CatId,
            request.FoodBrand,
            request.FoodType,
            request.QuantityGrams,
            request.DailyUsageGrams,
            request.LowStockThresholdDays);

        await _foodStockRepository.AddAsync(stock);

        return MapToResponse(stock);
    }

    public async Task UpdateFoodStockAsync(Guid id, UpdateFoodStockRequest request)
    {
        var stock = await _foodStockRepository.GetByIdAsync(id);
        if (stock is null)
            throw new KeyNotFoundException($"FoodStock with id '{id}' was not found.");

        stock.Update(request.QuantityGrams, request.DailyUsageGrams, request.LowStockThresholdDays);

        await _foodStockRepository.UpdateAsync(stock);
    }

    public async Task DeleteFoodStockAsync(Guid id)
    {
        await _foodStockRepository.DeleteAsync(id);
    }

    public async Task<IReadOnlyList<string>> GetBrandSuggestionsAsync(Guid catId)
    {
        return await _feedingLogRepository.GetDistinctBrandsAsync(catId);
    }

    private static FeedingLogResponse MapToResponse(FeedingLog log)
    {
        return new FeedingLogResponse(
            log.Id,
            log.CatId,
            log.LoggedAt,
            log.FoodBrand,
            log.FoodType,
            log.AmountGrams,
            log.Notes);
    }

    private FoodStockResponse MapToResponse(FoodStock stock)
    {
        return new FoodStockResponse(
            stock.Id,
            stock.CatId,
            stock.FoodBrand,
            stock.FoodType,
            stock.QuantityGrams,
            stock.DailyUsageGrams,
            stock.LowStockThresholdDays,
            stock.UpdatedAt,
            _foodStockService.DaysRemaining(stock),
            _foodStockService.IsLowStock(stock));
    }
}
