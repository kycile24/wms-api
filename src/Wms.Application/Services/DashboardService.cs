using Microsoft.EntityFrameworkCore;
using Wms.Application.DTOs.Dashboard;
using Wms.Application.Interfaces;
using Wms.Domain.Common;
using Wms.Domain.Entities;
using Wms.Domain.Enums;

namespace Wms.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DashboardSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var totalProducts = await _unitOfWork.Repository<Product>().Query().CountAsync(cancellationToken);

        // Підраховуємо загальну вартість товарів на складах
        var inventoryItems = await _unitOfWork.Repository<InventoryItem>()
            .Query()
            .AsNoTracking()
            .Include(i => i.Product)
            .ToListAsync(cancellationToken);

        var totalInventoryValue = inventoryItems.Sum(i => i.QuantityOnHand * (i.Product?.UnitPrice ?? 0m));

        // Позиції з недостатнім залишком (за замовчуванням ліміт 10, якщо не вказано інше)
        var lowStockCount = inventoryItems.Count(i => i.QuantityAvailable <= 10);

        var pendingPurchaseOrders = await _unitOfWork.Repository<PurchaseOrder>()
            .Query()
            .CountAsync(p => p.Status == OrderStatus.Pending || p.Status == OrderStatus.Processing, cancellationToken);

        var activeSalesOrders = await _unitOfWork.Repository<SalesOrder>()
            .Query()
            .CountAsync(s => s.Status == OrderStatus.Pending || s.Status == OrderStatus.Processing, cancellationToken);

        var recentMovements = await _unitOfWork.Repository<StockMovement>()
            .Query()
            .AsNoTracking()
            .Include(m => m.Product)
            .OrderByDescending(m => m.CreatedAtUtc)
            .Take(5)
            .Select(m => new RecentStockMovementDto(
                m.Id,
                m.Product != null ? m.Product.Name : string.Empty,
                m.Product != null ? m.Product.Sku : string.Empty,
                m.MovementType,
                m.Quantity,
                m.CreatedAtUtc,
                m.ReferenceNumber
            ))
            .ToListAsync(cancellationToken);

        var summary = new DashboardSummaryDto(
            totalProducts,
            totalInventoryValue,
            lowStockCount,
            pendingPurchaseOrders,
            activeSalesOrders,
            recentMovements
        );

        return Result<DashboardSummaryDto>.Success(summary);
    }

    public async Task<Result<IReadOnlyList<LowStockReportItemDto>>> GetLowStockAlertsAsync(CancellationToken cancellationToken = default)
    {
        const int defaultReorderLevel = 10;

        var lowStockItems = await _unitOfWork.Repository<InventoryItem>()
            .Query()
            .AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.QuantityAvailable <= defaultReorderLevel)
            .Select(i => new LowStockReportItemDto(
                i.ProductId,
                i.Product != null ? i.Product.Name : string.Empty,
                i.Product != null ? i.Product.Sku : string.Empty,
                i.WarehouseId,
                i.Warehouse != null ? i.Warehouse.Name : string.Empty,
                i.QuantityOnHand,
                i.QuantityAvailable,
                defaultReorderLevel
            ))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<LowStockReportItemDto>>.Success(lowStockItems);
    }

    public async Task<Result<IReadOnlyList<StockMovementReportDto>>> GetStockMovementReportAsync(
        DateTime? fromDateUtc,
        DateTime? toDateUtc,
        Guid? warehouseId,
        CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<StockMovement>()
            .Query()
            .AsNoTracking()
            .Include(m => m.Product)
            .Include(m => m.SourceWarehouse)
            .Include(m => m.DestinationWarehouse)
            .AsQueryable();

        if (fromDateUtc.HasValue)
            query = query.Where(m => m.CreatedAtUtc >= fromDateUtc.Value);

        if (toDateUtc.HasValue)
            query = query.Where(m => m.CreatedAtUtc <= toDateUtc.Value);

        if (warehouseId.HasValue)
            query = query.Where(m => m.SourceWarehouseId == warehouseId.Value || m.DestinationWarehouseId == warehouseId.Value);

        var report = await query
            .OrderByDescending(m => m.CreatedAtUtc)
            .Select(m => new StockMovementReportDto(
                m.Id,
                m.Product != null ? m.Product.Name : string.Empty,
                m.Product != null ? m.Product.Sku : string.Empty,
                m.SourceWarehouse != null ? m.SourceWarehouse.Name : (m.DestinationWarehouse != null ? m.DestinationWarehouse.Name : string.Empty),
                m.MovementType,
                m.Quantity,
                m.CreatedAtUtc,
                m.ReferenceNumber,
                m.Reason
            ))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<StockMovementReportDto>>.Success(report);
    }
}