using Wms.Application.DTOs.PurchaseOrders;
using Wms.Domain.Common;
using Wms.Domain.Enums;

namespace Wms.Application.Interfaces;

public interface IPurchaseOrderService
{
    Task<Result<PurchaseOrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<PurchaseOrderDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateAsync(CreatePurchaseOrderDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken cancellationToken = default);
    Task<Result> ReceiveItemsAsync(Guid orderId, ReceivePurchaseOrderDto dto, CancellationToken cancellationToken = default);
}