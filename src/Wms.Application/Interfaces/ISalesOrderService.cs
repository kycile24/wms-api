using Wms.Application.DTOs.SalesOrders;
using Wms.Domain.Common;
using Wms.Domain.Enums;

namespace Wms.Application.Interfaces;

public interface ISalesOrderService
{
    Task<Result<SalesOrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<SalesOrderDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateAsync(CreateSalesOrderDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken cancellationToken = default);
    Task<Result<Guid>> ShipOrderAsync(Guid orderId, CreateShipmentDto dto, CancellationToken cancellationToken = default);
    Task<Result> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}