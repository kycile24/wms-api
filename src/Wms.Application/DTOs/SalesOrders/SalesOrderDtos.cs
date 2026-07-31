using Wms.Domain.Enums;

namespace Wms.Application.DTOs.SalesOrders;

public record SalesOrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    int QuantityOrdered,
    int QuantityShipped,
    decimal UnitPrice,
    decimal TotalPrice
);

public record ShipmentDto(
    Guid Id,
    string TrackingNumber,
    string Carrier,
    ShipmentStatus Status,
    DateTime? ShippedAtUtc,
    DateTime? DeliveredAtUtc
);

public record SalesOrderDto(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string CustomerName,
    Guid SourceWarehouseId,
    string SourceWarehouseName,
    OrderStatus Status,
    decimal TotalAmount,
    DateTime OrderDateUtc,
    IReadOnlyList<SalesOrderItemDto> Items,
    IReadOnlyList<ShipmentDto> Shipments
);

public record CreateSalesOrderItemDto(
    Guid ProductId,
    int QuantityOrdered,
    decimal UnitPrice
);

public record CreateSalesOrderDto(
    string OrderNumber,
    Guid CustomerId,
    Guid SourceWarehouseId,
    List<CreateSalesOrderItemDto> Items
);

public record CreateShipmentDto(
    string TrackingNumber,
    string Carrier
);