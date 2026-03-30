using CatTracker.Domain.Entities;

namespace CatTracker.Domain.Interfaces;

public interface IVetRecordRepository
{
    Task<IReadOnlyList<VetRecord>> GetByCatAsync(Guid catId);
    Task<IReadOnlyList<VetRecord>> GetUpcomingAsync(Guid catId, DateOnly withinDays);
    Task<VetRecord?> GetByIdAsync(Guid id);
    Task AddAsync(VetRecord record);
    Task UpdateAsync(VetRecord record);
    Task DeleteAsync(Guid id);
}
