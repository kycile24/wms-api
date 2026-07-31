using Microsoft.EntityFrameworkCore;
using Wms.Application.DTOs.Inventory;
using Wms.Application.Interfaces;
using Wms.Domain.Common;
using Wms.Domain.Entities;
using Wms.Domain.Enums;

namespace Wms.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public InventoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<InventoryItemDto>>> GetStockByWarehouseAsync(Guid warehouseId, CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Repository<InventoryItem>()
            .Query()
            .AsNoTracking()
            .Include(i => i.Warehouse)
            .Include(i => i.Product)
            .Where(i => i.WarehouseId == warehouseId)
            .Select(i => MapToInventoryDto(i))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<InventoryItemDto>>.Success(items);
    }

    public async Task<Result<IReadOnlyList<InventoryItemDto>>> GetStockByProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Repository<InventoryItem>()
            .Query()
            .AsNoTracking()
            .Include(i => i.Warehouse)
            .Include(i => i.Product)
            .Where(i => i.ProductId == productId)
            .Select(i => MapToInventoryDto(i))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<InventoryItemDto>>.Success(items);
    }

    public async Task<Result<IReadOnlyList<StockMovementDto>>> GetMovementsAsync(Guid? productId, Guid? warehouseId, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<StockMovement>()
            .Query()
            .AsNoTracking()
            .Include(m => m.Product)
            .Include(m => m.SourceWarehouse)
            .Include(m => m.DestinationWarehouse)
            .AsQueryable();

        if (productId.HasValue)
            query = query.Where(m => m.ProductId == productId.Value);

        if (warehouseId.HasValue)
            query = query.Where(m => m.SourceWarehouseId == warehouseId.Value || m.DestinationWarehouseId == warehouseId.Value);

        var movements = await query
            .OrderByDescending(m => m.CreatedAtUtc)
            .Select(m => new StockMovementDto(
                m.Id,
                m.ProductId,
                m.Product.Name,
                m.SourceWarehouseId,
                m.SourceWarehouse != null ? m.SourceWarehouse.Name : null,
                m.DestinationWarehouseId,
                m.DestinationWarehouse != null ? m.DestinationWarehouse.Name : null,
                m.Quantity,
                m.MovementType,
                m.ReferenceNumber,
                m.Reason,
                m.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<StockMovementDto>>.Success(movements);
    }

    public async Task<Result<Guid>> RegisterMovementAsync(CreateStockMovementDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Quantity <= 0)
            return Result<Guid>.Failure(new Error("Inventory.InvalidQuantity", "Кількість товару повинна бути більшою за нуль."));

        var productExists = await _unitOfWork.Repository<Product>().ExistsAsync(p => p.Id == dto.ProductId, cancellationToken);
        if (!productExists)
            return Result<Guid>.Failure(new Error("Product.NotFound", "Вказаний товар не знайдено."));

        // Логіка оновлення залишків залежно від типу переміщення
        switch (dto.MovementType)
        {
            case MovementType.Inbound:
                if (!dto.DestinationWarehouseId.HasValue)
                    return Result<Guid>.Failure(new Error("Inventory.MissingDestination", "Для приходу товару необхідно вказати склад призначення."));

                await AddStockAsync(dto.DestinationWarehouseId.Value, dto.ProductId, dto.Quantity, dto.Zone, dto.Aisle, dto.Rack, dto.Shelf, cancellationToken);
                break;

            case MovementType.Outbound:
                if (!dto.SourceWarehouseId.HasValue)
                    return Result<Guid>.Failure(new Error("Inventory.MissingSource", "Для списання товару необхідно вказати склад списання."));

                var removeResult = await RemoveStockAsync(dto.SourceWarehouseId.Value, dto.ProductId, dto.Quantity, cancellationToken);
                if (!removeResult.IsSuccess)
                    return Result<Guid>.Failure(removeResult.Error);
                break;

            case MovementType.Transfer:
                if (!dto.SourceWarehouseId.HasValue || !dto.DestinationWarehouseId.HasValue)
                    return Result<Guid>.Failure(new Error("Inventory.MissingWarehouses", "Для переміщення необхідно вказати склад списання та склад призначення."));

                if (dto.SourceWarehouseId == dto.DestinationWarehouseId)
                    return Result<Guid>.Failure(new Error("Inventory.SameWarehouse", "Склад призначення не може збігатися зі складом списання."));

                var transferRemoveResult = await RemoveStockAsync(dto.SourceWarehouseId.Value, dto.ProductId, dto.Quantity, cancellationToken);
                if (!transferRemoveResult.IsSuccess)
                    return Result<Guid>.Failure(transferRemoveResult.Error);

                await AddStockAsync(dto.DestinationWarehouseId.Value, dto.ProductId, dto.Quantity, dto.Zone, dto.Aisle, dto.Rack, dto.Shelf, cancellationToken);
                break;

            case MovementType.Adjustment:
                if (dto.DestinationWarehouseId.HasValue)
                    await AddStockAsync(dto.DestinationWarehouseId.Value, dto.ProductId, dto.Quantity, dto.Zone, dto.Aisle, dto.Rack, dto.Shelf, cancellationToken);
                else if (dto.SourceWarehouseId.HasValue)
                {
                    var adjResult = await RemoveStockAsync(dto.SourceWarehouseId.Value, dto.ProductId, dto.Quantity, cancellationToken);
                    if (!adjResult.IsSuccess)
                        return Result<Guid>.Failure(adjResult.Error);
                }
                else
                    return Result<Guid>.Failure(new Error("Inventory.MissingWarehouse", "Вкажіть склад для проведення коригування."));
                break;

            default:
                return Result<Guid>.Failure(new Error("Inventory.InvalidMovementType", "Невідомий тип переміщення."));
        }

        // Фіксація історії
        var movement = new StockMovement
        {
            ProductId = dto.ProductId,
            SourceWarehouseId = dto.SourceWarehouseId,
            DestinationWarehouseId = dto.DestinationWarehouseId,
            Quantity = dto.Quantity,
            MovementType = dto.MovementType,
            ReferenceNumber = dto.ReferenceNumber.Trim(),
            Reason = dto.Reason.Trim()
        };

        await _unitOfWork.Repository<StockMovement>().AddAsync(movement, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(movement.Id);
    }

    private async Task AddStockAsync(Guid warehouseId, Guid productId, int quantity, string zone, string aisle, string rack, string shelf, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<InventoryItem>();
        var item = await repo.Query().FirstOrDefaultAsync(i => i.WarehouseId == warehouseId && i.ProductId == productId, cancellationToken);

        if (item is null)
        {
            item = new InventoryItem
            {
                WarehouseId = warehouseId,
                ProductId = productId,
                QuantityOnHand = quantity,
                QuantityAllocated = 0,
                Zone = zone,
                Aisle = aisle,
                Rack = rack,
                Shelf = shelf
            };
            await repo.AddAsync(item, cancellationToken);
        }
        else
        {
            item.QuantityOnHand += quantity;
            repo.Update(item);
        }
    }

    private async Task<Result> RemoveStockAsync(Guid warehouseId, Guid productId, int quantity, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<InventoryItem>();
        var item = await repo.Query().FirstOrDefaultAsync(i => i.WarehouseId == warehouseId && i.ProductId == productId, cancellationToken);

        if (item is null || item.QuantityAvailable < quantity)
        {
            return Result.Failure(new Error("Inventory.InsufficientStock", $"Недостатньо доступного товару на складі. Доступно: {item?.QuantityAvailable ?? 0}."));
        }

        item.QuantityOnHand -= quantity;
        repo.Update(item);
        return Result.Success();
    }

    private static InventoryItemDto MapToInventoryDto(InventoryItem i) => new(
        i.Id,
        i.WarehouseId,
        i.Warehouse != null ? i.Warehouse.Name : string.Empty,
        i.ProductId,
        i.Product != null ? i.Product.Sku : string.Empty,
        i.Product != null ? i.Product.Name : string.Empty,
        i.QuantityOnHand,
        i.QuantityAllocated,
        i.QuantityAvailable,
        i.Zone,
        i.Aisle,
        i.Rack,
        i.Shelf
    );
}