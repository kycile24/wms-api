using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.Common;
using Wms.Application.DTOs.Inventory;
using Wms.Application.Interfaces;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController(IInventoryService inventoryService) : ControllerBase
{
    [HttpGet("warehouse/{warehouseId:guid}")]
    public async Task<IActionResult> GetStockByWarehouse(Guid warehouseId, CancellationToken ct)
    {
        var result = await inventoryService.GetStockByWarehouseAsync(warehouseId, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<IReadOnlyList<InventoryItemDto>>.SuccessResponse(result.Value))
            : BadRequest(ApiResponse<IReadOnlyList<InventoryItemDto>>.Failure(result.Error.Message));
    }

    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetStockByProduct(Guid productId, CancellationToken ct)
    {
        var result = await inventoryService.GetStockByProductAsync(productId, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<IReadOnlyList<InventoryItemDto>>.SuccessResponse(result.Value))
            : BadRequest(ApiResponse<IReadOnlyList<InventoryItemDto>>.Failure(result.Error.Message));
    }

    [HttpGet("movements")]
    public async Task<IActionResult> GetMovements([FromQuery] Guid? productId, [FromQuery] Guid? warehouseId, CancellationToken ct)
    {
        var result = await inventoryService.GetMovementsAsync(productId, warehouseId, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<IReadOnlyList<StockMovementDto>>.SuccessResponse(result.Value))
            : BadRequest(ApiResponse<IReadOnlyList<StockMovementDto>>.Failure(result.Error.Message));
    }

    [HttpPost("movements")]
    public async Task<IActionResult> RegisterMovement([FromBody] CreateStockMovementDto dto, CancellationToken ct)
    {
        var result = await inventoryService.RegisterMovementAsync(dto, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Складську операцію успішно проведено."))
            : BadRequest(ApiResponse<Guid>.Failure(result.Error.Message));
    }
}