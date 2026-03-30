using CatTracker.Application.DTOs;
using CatTracker.Application.Interfaces;
using CatTracker.Domain.Interfaces;

namespace CatTracker.Application.Services;

public class CatService : ICatService
{
    private readonly ICatRepository _catRepository;

    public CatService(ICatRepository catRepository)
    {
        _catRepository = catRepository;
    }

    public async Task<CatResponse?> GetCatAsync(Guid id)
    {
        var cat = await _catRepository.GetByIdAsync(id);
        if (cat is null)
            return null;

        return new CatResponse(
            cat.Id,
            cat.Name,
            cat.Breed,
            cat.DateOfBirth,
            cat.PhotoUrl,
            cat.IsActive,
            cat.CreatedAt);
    }

    public async Task<IReadOnlyList<CatResponse>> GetAllActiveCatsAsync()
    {
        var cats = await _catRepository.GetAllActiveAsync();

        return cats
            .Select(cat => new CatResponse(
                cat.Id,
                cat.Name,
                cat.Breed,
                cat.DateOfBirth,
                cat.PhotoUrl,
                cat.IsActive,
                cat.CreatedAt))
            .ToList();
    }

    public async Task UpdateCatAsync(Guid id, UpdateCatRequest request)
    {
        var cat = await _catRepository.GetByIdAsync(id);
        if (cat is null)
            throw new KeyNotFoundException($"Cat with id '{id}' was not found.");

        cat.Update(request.Name, request.Breed, request.DateOfBirth, request.PhotoUrl);

        await _catRepository.UpdateAsync(cat);
    }
}
