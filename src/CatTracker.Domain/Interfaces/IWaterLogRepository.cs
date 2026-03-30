using CatTracker.Domain.Entities;

namespace CatTracker.Domain.Interfaces;

public interface IWaterLogRepository
{
    Task<IReadOnlyList<WaterLog>> GetRecentAsync(Guid catId, int count = 20);
    Task<WaterLog?> GetLatestAsync(Guid catId);
    Task AddAsync(WaterLog log);
    Task DeleteAsync(Guid id);
}
