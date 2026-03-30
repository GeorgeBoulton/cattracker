using CatTracker.Application.DTOs;
using CatTracker.Application.Interfaces;
using CatTracker.Domain.Entities;
using CatTracker.Domain.Interfaces;

namespace CatTracker.Application.Services;

public class WaterService : IWaterService
{
    private readonly IWaterLogRepository _waterLogRepository;

    public WaterService(IWaterLogRepository waterLogRepository)
    {
        _waterLogRepository = waterLogRepository;
    }

    public async Task<WaterLogResponse> LogWaterAsync(LogWaterRequest request)
    {
        var log = WaterLog.Create(
            request.CatId,
            request.Notes,
            request.CleanedAt);

        await _waterLogRepository.AddAsync(log);

        return MapToResponse(log);
    }

    public async Task<IReadOnlyList<WaterLogResponse>> GetRecentAsync(Guid catId, int count = 20)
    {
        var logs = await _waterLogRepository.GetRecentAsync(catId, count);

        return logs
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<WaterLogResponse?> GetLatestAsync(Guid catId)
    {
        var log = await _waterLogRepository.GetLatestAsync(catId);
        if (log is null)
            return null;

        return MapToResponse(log);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _waterLogRepository.DeleteAsync(id);
    }

    private static WaterLogResponse MapToResponse(WaterLog log)
    {
        return new WaterLogResponse(
            log.Id,
            log.CatId,
            log.CleanedAt,
            log.Notes);
    }
}
