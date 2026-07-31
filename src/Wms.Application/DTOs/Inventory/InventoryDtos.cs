using Wms.Domain.Enums;

namespace Wms.Application.DTOs.Inventory;

public record InventoryItemDto(
    Guid Id,
    Guid WarehouseId,
    string WarehouseName,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    int QuantityOnHand,
    int QuantityAllocated,
    int QuantityAvailable,
    string Zone,
    string Aisle,
    string Rack,
    string Shelf
);

public record StockMovementDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    Guid? SourceWarehouseId,
    string? SourceWarehouseName,
    Guid? DestinationWarehouseId,
    string? DestinationWarehouseName,
    int Quantity,
    MovementType MovementType,
    string ReferenceNumber,
    string Reason,
    DateTime CreatedAtUtc
);

public record CreateStockMovementDto(
    Guid ProductId,
    Guid? SourceWarehouseId,
    Guid? DestinationWarehouseId,
    int Quantity,
    MovementType MovementType,
    string ReferenceNumber,
    string Reason,
    string Zone = "Default",
    string Aisle = "01",
    string Rack = "01",
    string Shelf = "01"
);