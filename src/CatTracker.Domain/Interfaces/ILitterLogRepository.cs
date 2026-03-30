using CatTracker.Domain.Entities;

namespace CatTracker.Domain.Interfaces;

public interface ILitterLogRepository
{
    Task<IReadOnlyList<LitterLog>> GetSinceAsync(Guid catId, DateTime since);
    Task<LitterLog?> GetLatestAsync(Guid catId);
    Task<LitterLog?> GetLatestFullChangeAsync(Guid catId);
    Task AddAsync(LitterLog log);
    Task DeleteAsync(Guid id);
}
