using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.Common;
using Wms.Application.DTOs.SalesOrders;
using Wms.Application.Interfaces;
using Wms.Domain.Enums;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesOrdersController(ISalesOrderService salesOrderService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await salesOrderService.GetByIdAsync(id, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<SalesOrderDto>.SuccessResponse(result.Value))
            : NotFound(ApiResponse<SalesOrderDto>.Failure(result.Error.Message));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await salesOrderService.GetAllAsync(ct);
        return result.IsSuccess
            ? Ok(ApiResponse<IReadOnlyList<SalesOrderDto>>.SuccessResponse(result.Value))
            : BadRequest(ApiResponse<IReadOnlyList<SalesOrderDto>>.Failure(result.Error.Message));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSalesOrderDto dto, CancellationToken ct)
    {
        var result = await salesOrderService.CreateAsync(dto, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Замовлення на продаж створено та товар зарезервовано."))
            : BadRequest(ApiResponse<Guid>.Failure(result.Error.Message));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] OrderStatus status, CancellationToken ct)
    {
        var result = await salesOrderService.UpdateStatusAsync(id, status, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<string>.SuccessResponse("Статус замовлення оновлено."))
            : BadRequest(ApiResponse<string>.Failure(result.Error.Message));
    }

    [HttpPost("{id:guid}/ship")]
    public async Task<IActionResult> ShipOrder(Guid id, [FromBody] CreateShipmentDto dto, CancellationToken ct)
    {
        var result = await salesOrderService.ShipOrderAsync(id, dto, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Замовлення успішно відвантажено, згенеровано ТТН."))
            : BadRequest(ApiResponse<Guid>.Failure(result.Error.Message));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id, CancellationToken ct)
    {
        var result = await salesOrderService.CancelOrderAsync(id, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<string>.SuccessResponse("Замовлення скасовано, резерв залишків знято."))
            : BadRequest(ApiResponse<string>.Failure(result.Error.Message));
    }
}