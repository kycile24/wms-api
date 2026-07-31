using Wms.Application.Common;
namespace Wms.Application.DTOs.Products;

public record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    string? Barcode,
    decimal UnitPrice,
    string UnitOfMeasure,
    int MinimumStockThreshold,
    string? ImageUrl,
    Guid CategoryId,
    string CategoryName,
    DateTime CreatedAtUtc
);

public record CreateProductDto(
    string Sku,
    string Name,
    string? Description,
    string? Barcode,
    decimal UnitPrice,
    string UnitOfMeasure,
    int MinimumStockThreshold,
    string? ImageUrl,
    Guid CategoryId
);

public record UpdateProductDto(
    string Sku,
    string Name,
    string? Description,
    string? Barcode,
    decimal UnitPrice,
    string UnitOfMeasure,
    int MinimumStockThreshold,
    string? ImageUrl,
    Guid CategoryId
);