using Wms.Application.Common;
using Wms.Application.DTOs.Products;
using Wms.Domain.Common;

namespace Wms.Application.Interfaces;

public interface IProductService
{
    Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PagedList<ProductDto>>> GetPagedAsync(string? searchTerm, Guid? categoryId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}