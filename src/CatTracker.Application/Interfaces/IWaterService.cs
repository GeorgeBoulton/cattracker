using CatTracker.Application.DTOs;

namespace CatTracker.Application.Interfaces;

public interface IWaterService
{
    Task<WaterLogResponse> LogWaterAsync(LogWaterRequest request);
    Task<IReadOnlyList<WaterLogResponse>> GetRecentAsync(Guid catId, int count = 20);
    Task<WaterLogResponse?> GetLatestAsync(Guid catId);
    Task DeleteAsync(Guid id);
}
