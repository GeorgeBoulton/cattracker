using CatTracker.Domain.Entities;

namespace CatTracker.Domain.Interfaces;

public interface ICatRepository
{
    Task<Cat?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Cat>> GetAllActiveAsync();
    Task AddAsync(Cat cat);
    Task UpdateAsync(Cat cat);
    Task DeleteAsync(Guid id);
}
