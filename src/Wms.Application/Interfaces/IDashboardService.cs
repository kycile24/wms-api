using Wms.Application.DTOs.Dashboard;
using Wms.Domain.Common;

namespace Wms.Application.Interfaces;

public interface IDashboardService
{
    Task<Result<DashboardSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<LowStockReportItemDto>>> GetLowStockAlertsAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<StockMovementReportDto>>> GetStockMovementReportAsync(
        DateTime? fromDateUtc,
        DateTime? toDateUtc,
        Guid? warehouseId,
        CancellationToken cancellationToken = default);
}