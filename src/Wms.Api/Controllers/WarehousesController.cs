using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.Common;
using Wms.Application.DTOs.Warehouses;
using Wms.Application.Interfaces;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WarehousesController(IWarehouseService warehouseService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await warehouseService.GetByIdAsync(id, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<WarehouseDto>.SuccessResponse(result.Value))
            : NotFound(ApiResponse<WarehouseDto>.Failure(result.Error.Message));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await warehouseService.GetAllAsync(ct);
        return result.IsSuccess
            ? Ok(ApiResponse<IReadOnlyList<WarehouseDto>>.SuccessResponse(result.Value))
            : BadRequest(ApiResponse<IReadOnlyList<WarehouseDto>>.Failure(result.Error.Message));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseDto dto, CancellationToken ct)
    {
        var result = await warehouseService.CreateAsync(dto, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Склад успішно створено."))
            : BadRequest(ApiResponse<Guid>.Failure(result.Error.Message));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseDto dto, CancellationToken ct)
    {
        var result = await warehouseService.UpdateAsync(id, dto, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<string>.SuccessResponse("Склад успішно оновлено."))
            : BadRequest(ApiResponse<string>.Failure(result.Error.Message));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await warehouseService.DeleteAsync(id, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<string>.SuccessResponse("Склад успішно видалено."))
            : BadRequest(ApiResponse<string>.Failure(result.Error.Message));
    }
}