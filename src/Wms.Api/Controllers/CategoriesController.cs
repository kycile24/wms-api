using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.Common;
using Wms.Application.DTOs.Categories;
using Wms.Application.Interfaces;

namespace Wms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await categoryService.GetByIdAsync(id, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<CategoryDto>.SuccessResponse(result.Value))
            : NotFound(ApiResponse<CategoryDto>.Failure(result.Error.Message));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await categoryService.GetAllAsync(ct);
        return result.IsSuccess
            ? Ok(ApiResponse<IReadOnlyList<CategoryDto>>.SuccessResponse(result.Value))
            : BadRequest(ApiResponse<IReadOnlyList<CategoryDto>>.Failure(result.Error.Message));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto, CancellationToken ct)
    {
        var result = await categoryService.CreateAsync(dto, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<Guid>.SuccessResponse(result.Value, "Категорію успішно створено."))
            : BadRequest(ApiResponse<Guid>.Failure(result.Error.Message));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryDto dto, CancellationToken ct)
    {
        var result = await categoryService.UpdateAsync(id, dto, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<string>.SuccessResponse("Категорію успішно оновлено."))
            : BadRequest(ApiResponse<string>.Failure(result.Error.Message));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await categoryService.DeleteAsync(id, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<string>.SuccessResponse("Категорію успішно видалено."))
            : BadRequest(ApiResponse<string>.Failure(result.Error.Message));
    }
}