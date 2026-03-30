using CatTracker.Application.DTOs;

namespace CatTracker.Application.Interfaces;

public interface ICatService
{
    Task<CatResponse?> GetCatAsync(Guid id);
    Task<IReadOnlyList<CatResponse>> GetAllActiveCatsAsync();
    Task UpdateCatAsync(Guid id, UpdateCatRequest request);
}
