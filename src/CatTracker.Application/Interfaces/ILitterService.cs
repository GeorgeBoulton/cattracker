using CatTracker.Application.DTOs;

namespace CatTracker.Application.Interfaces;

public interface ILitterService
{
    Task<LitterLogResponse> LogLitterAsync(LogLitterRequest request);
    Task<IReadOnlyList<LitterLogResponse>> GetSinceAsync(Guid catId, DateTime since);
    Task<LitterLogResponse?> GetLatestAsync(Guid catId);
    Task<LitterLogResponse?> GetLatestFullChangeAsync(Guid catId);
    Task DeleteAsync(Guid id);
}
