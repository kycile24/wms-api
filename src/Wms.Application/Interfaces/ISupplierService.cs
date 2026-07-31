using Wms.Application.DTOs.Suppliers;
using Wms.Domain.Common;

namespace Wms.Application.Interfaces;

public interface ISupplierService
{
    Task<Result<SupplierDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<SupplierDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateAsync(CreateSupplierDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Guid id, UpdateSupplierDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}