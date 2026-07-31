using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.Common;
using Wms.Application.DTOs.Suppliers;
using Wms.Application.Interfaces;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController(ISupplierService supplierService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await supplierService.GetByIdAsync(id, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<SupplierDto>.SuccessResponse(result.Value))
            : NotFound(ApiResponse<SupplierDto>.Failure(result.Error.Message));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await supplierService.GetAllAsync(ct);
        return result.IsSuccess
            ? Ok(ApiResponse<IReadOnlyList<SupplierDto>>.SuccessResponse(result.Value))
            : BadRequest(ApiResponse<IReadOnlyList<SupplierDto>>.Failure(result.Error.Message));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplierDto dto, CancellationToken ct)
    {
        var result = await supplierService.CreateAsync(dto, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Постачальника успішно створено."))
            : BadRequest(ApiResponse<Guid>.Failure(result.Error.Message));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierDto dto, CancellationToken ct)
    {
        var result = await supplierService.UpdateAsync(id, dto, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<string>.SuccessResponse("Постачальника успішно оновлено."))
            : BadRequest(ApiResponse<string>.Failure(result.Error.Message));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await supplierService.DeleteAsync(id, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<string>.SuccessResponse("Постачальника успішно видалено."))
            : BadRequest(ApiResponse<string>.Failure(result.Error.Message));
    }
}