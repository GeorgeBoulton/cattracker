using CatTracker.Application.DTOs;
using CatTracker.Application.Interfaces;
using CatTracker.Domain.Entities;
using CatTracker.Domain.Interfaces;

namespace CatTracker.Application.Services;

public class VetService : IVetService
{
    private readonly IVetRecordRepository _vetRecordRepository;

    public VetService(IVetRecordRepository vetRecordRepository)
    {
        _vetRecordRepository = vetRecordRepository;
    }

    public async Task<IReadOnlyList<VetRecordResponse>> GetByCatAsync(Guid catId)
    {
        var records = await _vetRecordRepository.GetByCatAsync(catId);

        return records
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<IReadOnlyList<VetRecordResponse>> GetUpcomingAsync(Guid catId, DateOnly upToDate)
    {
        var records = await _vetRecordRepository.GetUpcomingAsync(catId, upToDate);

        return records
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<VetRecordResponse?> GetByIdAsync(Guid id)
    {
        var record = await _vetRecordRepository.GetByIdAsync(id);
        if (record is null)
            return null;

        return MapToResponse(record);
    }

    public async Task<VetRecordResponse> CreateAsync(CreateVetRecordRequest request)
    {
        var record = VetRecord.Create(
            request.CatId,
            request.RecordType,
            request.Date,
            request.Description,
            request.ClinicName,
            request.VetName,
            request.Notes,
            request.Cost,
            request.NextDueDate);

        await _vetRecordRepository.AddAsync(record);

        return MapToResponse(record);
    }

    public async Task UpdateAsync(Guid id, UpdateVetRecordRequest request)
    {
        var record = await _vetRecordRepository.GetByIdAsync(id);
        if (record is null)
            throw new KeyNotFoundException($"VetRecord with id {id} was not found.");

        record.Update(
            request.RecordType,
            request.Date,
            request.Description,
            request.ClinicName,
            request.VetName,
            request.Notes,
            request.Cost,
            request.NextDueDate);

        await _vetRecordRepository.UpdateAsync(record);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _vetRecordRepository.DeleteAsync(id);
    }

    private static VetRecordResponse MapToResponse(VetRecord record)
    {
        return new VetRecordResponse(
            record.Id,
            record.CatId,
            record.RecordType,
            record.Date,
            record.ClinicName,
            record.VetName,
            record.Description,
            record.Notes,
            record.Cost,
            record.NextDueDate);
    }
}
