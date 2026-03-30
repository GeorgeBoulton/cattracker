using CatTracker.Application.DTOs;

namespace CatTracker.Application.Interfaces;

public interface IVetService
{
    Task<IReadOnlyList<VetRecordResponse>> GetByCatAsync(Guid catId);
    Task<IReadOnlyList<VetRecordResponse>> GetUpcomingAsync(Guid catId, DateOnly upToDate);
    Task<VetRecordResponse?> GetByIdAsync(Guid id);
    Task<VetRecordResponse> CreateAsync(CreateVetRecordRequest request);
    Task UpdateAsync(Guid id, UpdateVetRecordRequest request);
    Task DeleteAsync(Guid id);
}
