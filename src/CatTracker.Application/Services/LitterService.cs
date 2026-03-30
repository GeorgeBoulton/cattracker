using CatTracker.Application.DTOs;
using CatTracker.Application.Interfaces;
using CatTracker.Domain.Entities;
using CatTracker.Domain.Interfaces;

namespace CatTracker.Application.Services;

public class LitterService : ILitterService
{
    private readonly ILitterLogRepository _litterLogRepository;

    public LitterService(ILitterLogRepository litterLogRepository)
    {
        _litterLogRepository = litterLogRepository;
    }

    public async Task<LitterLogResponse> LogLitterAsync(LogLitterRequest request)
    {
        var log = LitterLog.Create(
            request.CatId,
            request.EntryType,
            request.Notes,
            request.LoggedAt);

        await _litterLogRepository.AddAsync(log);

        return MapToResponse(log);
    }

    public async Task<IReadOnlyList<LitterLogResponse>> GetSinceAsync(Guid catId, DateTime since)
    {
        var logs = await _litterLogRepository.GetSinceAsync(catId, since);

        return logs
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<LitterLogResponse?> GetLatestAsync(Guid catId)
    {
        var log = await _litterLogRepository.GetLatestAsync(catId);
        if (log is null)
            return null;

        return MapToResponse(log);
    }

    public async Task<LitterLogResponse?> GetLatestFullChangeAsync(Guid catId)
    {
        var log = await _litterLogRepository.GetLatestFullChangeAsync(catId);
        if (log is null)
            return null;

        return MapToResponse(log);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _litterLogRepository.DeleteAsync(id);
    }

    private static LitterLogResponse MapToResponse(LitterLog log)
    {
        return new LitterLogResponse(
            log.Id,
            log.CatId,
            log.LoggedAt,
            log.EntryType,
            log.Notes);
    }
}
