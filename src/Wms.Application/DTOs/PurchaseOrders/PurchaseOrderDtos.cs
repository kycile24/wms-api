using Wms.Domain.Enums;

namespace Wms.Application.DTOs.PurchaseOrders;

public record PurchaseOrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    int QuantityOrdered,
    int QuantityReceived,
    decimal UnitCost,
    decimal TotalCost
);

public record PurchaseOrderDto(
    Guid Id,
    string OrderNumber,
    Guid SupplierId,
    string SupplierName,
    Guid DestinationWarehouseId,
    string DestinationWarehouseName,
    OrderStatus Status,
    decimal TotalAmount,
    DateTime OrderDateUtc,
    DateTime? ExpectedDeliveryDateUtc,
    IReadOnlyList<PurchaseOrderItemDto> Items
);

public record CreatePurchaseOrderItemDto(
    Guid ProductId,
    int QuantityOrdered,
    decimal UnitCost
);

public record CreatePurchaseOrderDto(
    string OrderNumber,
    Guid SupplierId,
    Guid DestinationWarehouseId,
    DateTime? ExpectedDeliveryDateUtc,
    List<CreatePurchaseOrderItemDto> Items
);

public record ReceiveOrderItemDto(
    Guid ItemId,
    int QuantityToReceive
);

public record ReceivePurchaseOrderDto(
    List<ReceiveOrderItemDto> ItemsToReceive
);