using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.Common;
using Wms.Application.DTOs.Products;
using Wms.Application.Interfaces;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await productService.GetByIdAsync(id, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<ProductDto>.SuccessResponse(result.Value))
            : NotFound(ApiResponse<ProductDto>.Failure(result.Error.Message));
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] string? searchTerm,
        [FromQuery] Guid? categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await productService.GetPagedAsync(searchTerm, categoryId, page, pageSize, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<PagedList<ProductDto>>.SuccessResponse(result.Value))
            : BadRequest(ApiResponse<PagedList<ProductDto>>.Failure(result.Error.Message));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken ct)
    {
        var result = await productService.CreateAsync(dto, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Товар успішно створено."))
            : BadRequest(ApiResponse<Guid>.Failure(result.Error.Message));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto, CancellationToken ct)
    {
        var result = await productService.UpdateAsync(id, dto, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<string>.SuccessResponse("Товар успішно оновлено."))
            : BadRequest(ApiResponse<string>.Failure(result.Error.Message));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await productService.DeleteAsync(id, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<string>.SuccessResponse("Товар успішно видалено."))
            : BadRequest(ApiResponse<string>.Failure(result.Error.Message));
    }
}