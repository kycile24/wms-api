using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.Common;
using Wms.Application.DTOs.Customers;
using Wms.Application.Interfaces;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController(ICustomerService customerService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await customerService.GetByIdAsync(id, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<CustomerDto>.SuccessResponse(result.Value))
            : NotFound(ApiResponse<CustomerDto>.Failure(result.Error.Message));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await customerService.GetAllAsync(ct);
        return result.IsSuccess
            ? Ok(ApiResponse<IReadOnlyList<CustomerDto>>.SuccessResponse(result.Value))
            : BadRequest(ApiResponse<IReadOnlyList<CustomerDto>>.Failure(result.Error.Message));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto, CancellationToken ct)
    {
        var result = await customerService.CreateAsync(dto, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Клієнта успішно створено."))
            : BadRequest(ApiResponse<Guid>.Failure(result.Error.Message));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerDto dto, CancellationToken ct)
    {
        var result = await customerService.UpdateAsync(id, dto, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<string>.SuccessResponse("Клієнта успішно оновлено."))
            : BadRequest(ApiResponse<string>.Failure(result.Error.Message));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await customerService.DeleteAsync(id, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<string>.SuccessResponse("Клієнта успішно видалено."))
            : BadRequest(ApiResponse<string>.Failure(result.Error.Message));
    }
}