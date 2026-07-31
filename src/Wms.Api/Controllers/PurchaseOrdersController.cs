using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.Common;
using Wms.Application.DTOs.PurchaseOrders;
using Wms.Application.Interfaces;
using Wms.Domain.Enums;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PurchaseOrdersController(IPurchaseOrderService purchaseOrderService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await purchaseOrderService.GetByIdAsync(id, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<PurchaseOrderDto>.SuccessResponse(result.Value))
            : NotFound(ApiResponse<PurchaseOrderDto>.Failure(result.Error.Message));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await purchaseOrderService.GetAllAsync(ct);
        return result.IsSuccess
            ? Ok(ApiResponse<IReadOnlyList<PurchaseOrderDto>>.SuccessResponse(result.Value))
            : BadRequest(ApiResponse<IReadOnlyList<PurchaseOrderDto>>.Failure(result.Error.Message));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderDto dto, CancellationToken ct)
    {
        var result = await purchaseOrderService.CreateAsync(dto, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Замовлення на закупівлю успішно створено."))
            : BadRequest(ApiResponse<Guid>.Failure(result.Error.Message));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] OrderStatus status, CancellationToken ct)
    {
        var result = await purchaseOrderService.UpdateStatusAsync(id, status, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<string>.SuccessResponse("Статус замовлення оновлено."))
            : BadRequest(ApiResponse<string>.Failure(result.Error.Message));
    }

    [HttpPost("{id:guid}/receive")]
    public async Task<IActionResult> ReceiveItems(Guid id, [FromBody] ReceivePurchaseOrderDto dto, CancellationToken ct)
    {
        var result = await purchaseOrderService.ReceiveItemsAsync(id, dto, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<string>.SuccessResponse("Товари успішно прийнято на склад."))
            : BadRequest(ApiResponse<string>.Failure(result.Error.Message));
    }
}