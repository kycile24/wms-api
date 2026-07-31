using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.Common;
using Wms.Application.DTOs.Dashboard;
using Wms.Application.Interfaces;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var result = await dashboardService.GetSummaryAsync(ct);
        return result.IsSuccess
            ? Ok(ApiResponse<DashboardSummaryDto>.SuccessResponse(result.Value))
            : BadRequest(ApiResponse<DashboardSummaryDto>.Failure(result.Error.Message));
    }

    [HttpGet("low-stock-alerts")]
    public async Task<IActionResult> GetLowStockAlerts(CancellationToken ct)
    {
        var result = await dashboardService.GetLowStockAlertsAsync(ct);
        return result.IsSuccess
            ? Ok(ApiResponse<IReadOnlyList<LowStockReportItemDto>>.SuccessResponse(result.Value))
            : BadRequest(ApiResponse<IReadOnlyList<LowStockReportItemDto>>.Failure(result.Error.Message));
    }

    [HttpGet("stock-movements-report")]
    public async Task<IActionResult> GetStockMovementReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] Guid? warehouseId,
        CancellationToken ct)
    {
        var result = await dashboardService.GetStockMovementReportAsync(fromDate, toDate, warehouseId, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<IReadOnlyList<StockMovementReportDto>>.SuccessResponse(result.Value))
            : BadRequest(ApiResponse<IReadOnlyList<StockMovementReportDto>>.Failure(result.Error.Message));
    }
}