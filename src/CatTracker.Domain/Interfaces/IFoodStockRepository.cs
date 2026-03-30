using CatTracker.Domain.Entities;

namespace CatTracker.Domain.Interfaces;

public interface IFoodStockRepository
{
    Task<IReadOnlyList<FoodStock>> GetByCatAsync(Guid catId);
    Task<FoodStock?> GetByIdAsync(Guid id);
    Task AddAsync(FoodStock stock);
    Task UpdateAsync(FoodStock stock);
    Task DeleteAsync(Guid id);
}
