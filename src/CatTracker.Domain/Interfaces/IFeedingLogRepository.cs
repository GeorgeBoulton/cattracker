using CatTracker.Domain.Entities;

namespace CatTracker.Domain.Interfaces;

public interface IFeedingLogRepository
{
    Task<IReadOnlyList<FeedingLog>> GetRecentAsync(Guid catId, int count = 20);
    Task<FeedingLog?> GetLatestAsync(Guid catId);
    Task<IReadOnlyList<string>> GetDistinctBrandsAsync(Guid catId);
    Task AddAsync(FeedingLog log);
    Task DeleteAsync(Guid id);
}
