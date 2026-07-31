using Wms.Application.DTOs.Categories;
using Wms.Domain.Common;

namespace Wms.Application.Interfaces;

public interface ICategoryService
{
    Task<Result<CategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}