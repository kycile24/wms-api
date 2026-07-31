using Microsoft.EntityFrameworkCore;
using Wms.Application.Common;
using Wms.Application.DTOs.Products;
using Wms.Application.Interfaces;
using Wms.Domain.Common;
using Wms.Domain.Entities;

namespace Wms.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Repository<Product>()
            .Query()
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
            return Result<ProductDto>.Failure($"Товар з ID '{id}' не знайдено.");

        return Result<ProductDto>.Success(MapToDto(product));
    }

    public async Task<Result<PagedList<ProductDto>>> GetPagedAsync(
        string? searchTerm,
        Guid? categoryId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<Product>()
            .Query()
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Sku.ToLower().Contains(term) ||
                (p.Barcode != null && p.Barcode.Contains(term)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => MapToDto(p))
            .ToListAsync(cancellationToken);

        var pagedList = new PagedList<ProductDto>(items, totalCount, pageNumber, pageSize);
        return Result<PagedList<ProductDto>>.Success(pagedList);
    }

    public async Task<Result<Guid>> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        var productRepo = _unitOfWork.Repository<Product>();
        var skuNormalized = dto.Sku.Trim().ToUpperInvariant();

        var skuExists = await productRepo.ExistsAsync(p => p.Sku == skuNormalized, cancellationToken);
        if (skuExists)
            return Result<Guid>.Failure($"Товар із SKU '{dto.Sku}' вже існує.");

        var categoryExists = await _unitOfWork.Repository<Category>().ExistsAsync(c => c.Id == dto.CategoryId, cancellationToken);
        if (!categoryExists)
            return Result<Guid>.Failure("Вказану категорію товару не знайдено.");

        var product = new Product
        {
            Sku = skuNormalized,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Barcode = dto.Barcode?.Trim(),
            UnitPrice = dto.UnitPrice,
            UnitOfMeasure = string.IsNullOrWhiteSpace(dto.UnitOfMeasure) ? "PCS" : dto.UnitOfMeasure.Trim().ToUpperInvariant(),
            MinimumStockThreshold = dto.MinimumStockThreshold,
            ImageUrl = dto.ImageUrl?.Trim(),
            CategoryId = dto.CategoryId
        };

        await productRepo.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(product.Id);
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var productRepo = _unitOfWork.Repository<Product>();
        var product = await productRepo.GetByIdAsync(id, cancellationToken);

        if (product is null)
            return Result.Failure($"Товар з ID '{id}' не знайдено.");

        var skuNormalized = dto.Sku.Trim().ToUpperInvariant();
        if (product.Sku != skuNormalized)
        {
            var skuExists = await productRepo.ExistsAsync(p => p.Sku == skuNormalized && p.Id != id, cancellationToken);
            if (skuExists)
                return Result.Failure($"Товар із SKU '{dto.Sku}' вже існує.");
        }

        var categoryExists = await _unitOfWork.Repository<Category>().ExistsAsync(c => c.Id == dto.CategoryId, cancellationToken);
        if (!categoryExists)
            return Result.Failure("Вказану категорію товару не знайдено.");

        product.Sku = skuNormalized;
        product.Name = dto.Name.Trim();
        product.Description = dto.Description?.Trim();
        product.Barcode = dto.Barcode?.Trim();
        product.UnitPrice = dto.UnitPrice;
        product.UnitOfMeasure = dto.UnitOfMeasure.Trim().ToUpperInvariant();
        product.MinimumStockThreshold = dto.MinimumStockThreshold;
        product.ImageUrl = dto.ImageUrl?.Trim();
        product.CategoryId = dto.CategoryId;

        productRepo.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.Repository<Product>();
        var product = await repository.GetByIdAsync(id, cancellationToken);

        if (product is null)
            return Result.Failure($"Товар з ID '{id}' не знайдено.");

        repository.Remove(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static ProductDto MapToDto(Product p) => new(
        p.Id,
        p.Sku,
        p.Name,
        p.Description,
        p.Barcode,
        p.UnitPrice,
        p.UnitOfMeasure,
        p.MinimumStockThreshold,
        p.ImageUrl,
        p.CategoryId,
        p.Category != null ? p.Category.Name : string.Empty,
        DateTime.UtcNow
    );
}