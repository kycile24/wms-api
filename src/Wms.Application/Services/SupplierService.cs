using Microsoft.EntityFrameworkCore;
using Wms.Application.DTOs.Suppliers;
using Wms.Application.Interfaces;
using Wms.Domain.Common;
using Wms.Domain.Entities;

namespace Wms.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _unitOfWork;

    public SupplierService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SupplierDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var supplier = await _unitOfWork.Repository<Supplier>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (supplier is null)
            return Result<SupplierDto>.Failure(new Error("Supplier.NotFound", $"Постачальника з ID '{id}' не знайдено."));

        return Result<SupplierDto>.Success(MapToDto(supplier));
    }

    public async Task<Result<IReadOnlyList<SupplierDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var suppliers = await _unitOfWork.Repository<Supplier>()
            .Query()
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => MapToDto(s))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<SupplierDto>>.Success(suppliers);
    }

    public async Task<Result<Guid>> CreateAsync(CreateSupplierDto dto, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Supplier>();

        var supplier = new Supplier
        {
            Name = dto.Name.Trim(),
            ContactPerson = dto.ContactPerson.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            Phone = dto.Phone.Trim(),
            Address = dto.Address?.Trim(),
            TaxId = dto.TaxId?.Trim()
        };

        await repo.AddAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(supplier.Id);
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateSupplierDto dto, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Supplier>();
        var supplier = await repo.GetByIdAsync(id, cancellationToken);

        if (supplier is null)
            return Result.Failure(new Error("Supplier.NotFound", $"Постачальника з ID '{id}' не знайдено."));

        supplier.Name = dto.Name.Trim();
        supplier.ContactPerson = dto.ContactPerson.Trim();
        supplier.Email = dto.Email.Trim().ToLowerInvariant();
        supplier.Phone = dto.Phone.Trim();
        supplier.Address = dto.Address?.Trim();
        supplier.TaxId = dto.TaxId?.Trim();

        repo.Update(supplier);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Supplier>();
        var supplier = await repo.GetByIdAsync(id, cancellationToken);

        if (supplier is null)
            return Result.Failure(new Error("Supplier.NotFound", $"Постачальника з ID '{id}' не знайдено."));

        repo.Remove(supplier);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static SupplierDto MapToDto(Supplier s) => new(
        s.Id,
        s.Name,
        s.ContactPerson,
        s.Email,
        s.Phone,
        s.Address,
        s.TaxId
    );
}