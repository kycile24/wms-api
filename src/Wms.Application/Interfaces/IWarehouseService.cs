using Wms.Application.DTOs.Warehouses;
using Wms.Domain.Common;

namespace Wms.Application.Interfaces;

public interface IWarehouseService
{
    Task<Result<WarehouseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<WarehouseDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateAsync(CreateWarehouseDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Guid id, UpdateWarehouseDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}