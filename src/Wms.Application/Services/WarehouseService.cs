using Microsoft.EntityFrameworkCore;
using Wms.Application.DTOs.Warehouses;
using Wms.Application.Interfaces;
using Wms.Domain.Common;
using Wms.Domain.Entities;

namespace Wms.Application.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IUnitOfWork _unitOfWork;

    public WarehouseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<WarehouseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var warehouse = await _unitOfWork.Repository<Warehouse>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

        if (warehouse is null)
            return Result<WarehouseDto>.Failure(new Error("Warehouse.NotFound", $"Склад з ID '{id}' не знайдено."));

        return Result<WarehouseDto>.Success(MapToDto(warehouse));
    }

    public async Task<Result<IReadOnlyList<WarehouseDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var warehouses = await _unitOfWork.Repository<Warehouse>()
            .Query()
            .AsNoTracking()
            .OrderBy(w => w.Name)
            .Select(w => MapToDto(w))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<WarehouseDto>>.Success(warehouses);
    }

    public async Task<Result<Guid>> CreateAsync(CreateWarehouseDto dto, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Warehouse>();
        var codeNormalized = dto.Code.Trim().ToUpperInvariant();

        var exists = await repo.ExistsAsync(w => w.Code == codeNormalized, cancellationToken);
        if (exists)
            return Result<Guid>.Failure(new Error("Warehouse.CodeExists", $"Склад з кодом '{dto.Code}' вже існує."));

        var warehouse = new Warehouse
        {
            Code = codeNormalized,
            Name = dto.Name.Trim(),
            Location = dto.Location.Trim(),
            TotalCapacityUnits = dto.TotalCapacityUnits,
            IsActive = true
        };

        await repo.AddAsync(warehouse, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(warehouse.Id);
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateWarehouseDto dto, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Warehouse>();
        var warehouse = await repo.GetByIdAsync(id, cancellationToken);

        if (warehouse is null)
            return Result.Failure(new Error("Warehouse.NotFound", $"Склад з ID '{id}' не знайдено."));

        warehouse.Name = dto.Name.Trim();
        warehouse.Location = dto.Location.Trim();
        warehouse.TotalCapacityUnits = dto.TotalCapacityUnits;
        warehouse.IsActive = dto.IsActive;

        repo.Update(warehouse);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Warehouse>();
        var warehouse = await repo.GetByIdAsync(id, cancellationToken);

        if (warehouse is null)
            return Result.Failure(new Error("Warehouse.NotFound", $"Склад з ID '{id}' не знайдено."));

        repo.Remove(warehouse);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static WarehouseDto MapToDto(Warehouse w) => new(
        w.Id,
        w.Code,
        w.Name,
        w.Location,
        w.TotalCapacityUnits,
        w.IsActive
    );
}