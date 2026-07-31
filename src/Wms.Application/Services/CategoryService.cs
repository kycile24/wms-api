using Microsoft.EntityFrameworkCore;
using Wms.Application.DTOs.Categories;
using Wms.Application.Interfaces;
using Wms.Domain.Common;
using Wms.Domain.Entities;

namespace Wms.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Repository<Category>()
            .Query()
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category is null)
            return Result<CategoryDto>.Failure($"Категорію з ID '{id}' не знайдено.");

        var dto = new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.ParentCategoryId,
            category.ParentCategory?.Name,
            DateTime.UtcNow
        );

        return Result<CategoryDto>.Success(dto);
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Repository<Category>()
            .Query()
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Description,
                c.ParentCategoryId,
                c.ParentCategory != null ? c.ParentCategory.Name : null,
                DateTime.UtcNow
            ))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CategoryDto>>.Success(categories);
    }

    public async Task<Result<Guid>> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<Category>();
        var nameNormalized = dto.Name.Trim();

        var exists = await repository.ExistsAsync(c => c.Name.ToLower() == nameNormalized.ToLower(), cancellationToken);
        if (exists)
            return Result<Guid>.Failure($"Категорія з назвою '{dto.Name}' вже існує.");

        if (dto.ParentCategoryId.HasValue)
        {
            var parentExists = await repository.ExistsAsync(c => c.Id == dto.ParentCategoryId.Value, cancellationToken);
            if (!parentExists)
                return Result<Guid>.Failure("Батьківську категорію не знайдено.");
        }

        var category = new Category
        {
            Name = nameNormalized,
            Description = dto.Description?.Trim(),
            ParentCategoryId = dto.ParentCategoryId
        };

        await repository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(category.Id);
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<Category>();
        var category = await repository.GetByIdAsync(id, cancellationToken);

        if (category is null)
            return Result.Failure($"Категорію з ID '{id}' не знайдено.");

        if (dto.ParentCategoryId.HasValue && dto.ParentCategoryId.Value == id)
            return Result.Failure("Категорія не може бути батьківською для самої себе.");

        category.Name = dto.Name.Trim();
        category.Description = dto.Description?.Trim();
        category.ParentCategoryId = dto.ParentCategoryId;

        repository.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<Category>();
        var category = await repository.GetByIdAsync(id, cancellationToken);

        if (category is null)
            return Result.Failure($"Категорію з ID '{id}' не знайдено.");

        repository.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}