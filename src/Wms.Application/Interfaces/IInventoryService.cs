using Wms.Application.DTOs.Inventory;
using Wms.Domain.Common;

namespace Wms.Application.Interfaces;

public interface IInventoryService
{
    Task<Result<IReadOnlyList<InventoryItemDto>>> GetStockByWarehouseAsync(Guid warehouseId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<InventoryItemDto>>> GetStockByProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<StockMovementDto>>> GetMovementsAsync(Guid? productId, Guid? warehouseId, CancellationToken cancellationToken = default);
    Task<Result<Guid>> RegisterMovementAsync(CreateStockMovementDto dto, CancellationToken cancellationToken = default);
}