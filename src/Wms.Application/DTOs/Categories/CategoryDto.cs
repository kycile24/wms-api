using Wms.Application.Common;
namespace Wms.Application.DTOs.Categories;

public record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    string? ParentCategoryName,
    DateTime CreatedAtUtc
);

public record CreateCategoryDto(
    string Name,
    string? Description,
    Guid? ParentCategoryId
);

public record UpdateCategoryDto(
    string Name,
    string? Description,
    Guid? ParentCategoryId
);