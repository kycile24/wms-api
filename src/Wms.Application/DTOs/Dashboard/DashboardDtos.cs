using Wms.Domain.Enums;

namespace Wms.Application.DTOs.Dashboard;

public record DashboardSummaryDto(
    int TotalProducts,
    decimal TotalInventoryValue,
    int LowStockItemsCount,
    int PendingPurchaseOrdersCount,
    int ActiveSalesOrdersCount,
    IReadOnlyList<RecentStockMovementDto> RecentMovements
);

public record RecentStockMovementDto(
    Guid Id,
    string ProductName,
    string ProductSku,
    MovementType MovementType,
    int Quantity,
    DateTime CreatedAtUtc,
    string? ReferenceNumber
);

public record LowStockReportItemDto(
    Guid ProductId,
    string ProductName,
    string ProductSku,
    Guid WarehouseId,
    string WarehouseName,
    int QuantityOnHand,
    int QuantityAvailable,
    int ReorderLevel
);

public record StockMovementReportDto(
    Guid Id,
    string ProductName,
    string ProductSku,
    string WarehouseName,
    MovementType MovementType,
    int Quantity,
    DateTime CreatedAtUtc,
    string? ReferenceNumber,
    string? Reason
);