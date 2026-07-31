using Microsoft.EntityFrameworkCore;
using Wms.Application.DTOs.PurchaseOrders;
using Wms.Application.Interfaces;
using Wms.Domain.Common;
using Wms.Domain.Entities;
using Wms.Domain.Enums;

namespace Wms.Application.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public PurchaseOrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PurchaseOrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Repository<PurchaseOrder>()
            .Query()
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.DestinationWarehouse)
            .Include(p => p.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (order is null)
            return Result<PurchaseOrderDto>.Failure(new Error("PurchaseOrder.NotFound", $"Замовлення з ID '{id}' не знайдено."));

        return Result<PurchaseOrderDto>.Success(MapToDto(order));
    }

    public async Task<Result<IReadOnlyList<PurchaseOrderDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Repository<PurchaseOrder>()
            .Query()
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.DestinationWarehouse)
            .Include(p => p.Items)
                .ThenInclude(i => i.Product)
            .OrderByDescending(p => p.OrderDateUtc)
            .Select(p => MapToDto(p))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<PurchaseOrderDto>>.Success(orders);
    }

    public async Task<Result<Guid>> CreateAsync(CreatePurchaseOrderDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Items is null || !dto.Items.Any())
            return Result<Guid>.Failure(new Error("PurchaseOrder.EmptyItems", "Замовлення повинно містити хоча б один товар."));

        var supplierExists = await _unitOfWork.Repository<Supplier>().ExistsAsync(s => s.Id == dto.SupplierId, cancellationToken);
        if (!supplierExists)
            return Result<Guid>.Failure(new Error("Supplier.NotFound", "Вказаного постачальника не знайдено."));

        var warehouseExists = await _unitOfWork.Repository<Warehouse>().ExistsAsync(w => w.Id == dto.DestinationWarehouseId, cancellationToken);
        if (!warehouseExists)
            return Result<Guid>.Failure(new Error("Warehouse.NotFound", "Вказаного склада призначення не знайдено."));

        var order = new PurchaseOrder
        {
            OrderNumber = dto.OrderNumber.Trim(),
            SupplierId = dto.SupplierId,
            DestinationWarehouseId = dto.DestinationWarehouseId,
            ExpectedDeliveryDateUtc = dto.ExpectedDeliveryDateUtc,
            Status = OrderStatus.Draft,
            OrderDateUtc = DateTime.UtcNow,
            TotalAmount = dto.Items.Sum(i => i.QuantityOrdered * i.UnitCost)
        };

        foreach (var itemDto in dto.Items)
        {
            order.Items.Add(new PurchaseOrderItem
            {
                ProductId = itemDto.ProductId,
                QuantityOrdered = itemDto.QuantityOrdered,
                QuantityReceived = 0,
                UnitCost = itemDto.UnitCost
            });
        }

        await _unitOfWork.Repository<PurchaseOrder>().AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(order.Id);
    }

    public async Task<Result> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<PurchaseOrder>();
        var order = await repo.GetByIdAsync(id, cancellationToken);

        if (order is null)
            return Result.Failure(new Error("PurchaseOrder.NotFound", $"Замовлення з ID '{id}' не знайдено."));

        order.Status = status;
        repo.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ReceiveItemsAsync(Guid orderId, ReceivePurchaseOrderDto dto, CancellationToken cancellationToken = default)
    {
        var orderRepo = _unitOfWork.Repository<PurchaseOrder>();
        var order = await orderRepo.Query()
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == orderId, cancellationToken);

        if (order is null)
            return Result.Failure(new Error("PurchaseOrder.NotFound", $"Замовлення з ID '{orderId}' не знайдено."));

        var inventoryRepo = _unitOfWork.Repository<InventoryItem>();
        var movementRepo = _unitOfWork.Repository<StockMovement>();

        foreach (var receiveItem in dto.ItemsToReceive)
        {
            var item = order.Items.FirstOrDefault(i => i.Id == receiveItem.ItemId);
            if (item is null) continue;

            if (receiveItem.QuantityToReceive <= 0) continue;

            item.QuantityReceived += receiveItem.QuantityToReceive;

            // Збільшуємо залишки на складі призначення
            var inventory = await inventoryRepo.Query()
                .FirstOrDefaultAsync(i => i.WarehouseId == order.DestinationWarehouseId && i.ProductId == item.ProductId, cancellationToken);

            if (inventory is null)
            {
                inventory = new InventoryItem
                {
                    WarehouseId = order.DestinationWarehouseId,
                    ProductId = item.ProductId,
                    QuantityOnHand = receiveItem.QuantityToReceive,
                    QuantityAllocated = 0,
                    Zone = "Default",
                    Aisle = "01",
                    Rack = "01",
                    Shelf = "01"
                };
                await inventoryRepo.AddAsync(inventory, cancellationToken);
            }
            else
            {
                inventory.QuantityOnHand += receiveItem.QuantityToReceive;
                inventoryRepo.Update(inventory);
            }

            // Фіксуємо транзакцію приходу в історії
            var movement = new StockMovement
            {
                ProductId = item.ProductId,
                DestinationWarehouseId = order.DestinationWarehouseId,
                Quantity = receiveItem.QuantityToReceive,
                MovementType = MovementType.Inbound,
                ReferenceNumber = order.OrderNumber,
                Reason = "Прийом замовлення на закупівлю"
            };
            await movementRepo.AddAsync(movement, cancellationToken);
        }

        // Авто-оновлення статусу замовлення
        var allReceived = order.Items.All(i => i.QuantityReceived >= i.QuantityOrdered);
        var anyReceived = order.Items.Any(i => i.QuantityReceived > 0);

        if (allReceived)
            order.Status = OrderStatus.Received; // або Completed
        else if (anyReceived)
            order.Status = OrderStatus.Pending; // В процесі прийому

        orderRepo.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static PurchaseOrderDto MapToDto(PurchaseOrder p) => new(
        p.Id,
        p.OrderNumber,
        p.SupplierId,
        p.Supplier != null ? p.Supplier.Name : string.Empty,
        p.DestinationWarehouseId,
        p.DestinationWarehouse != null ? p.DestinationWarehouse.Name : string.Empty,
        p.Status,
        p.TotalAmount,
        p.OrderDateUtc,
        p.ExpectedDeliveryDateUtc,
        p.Items.Select(i => new PurchaseOrderItemDto(
            i.Id,
            i.ProductId,
            i.Product != null ? i.Product.Name : string.Empty,
            i.Product != null ? i.Product.Sku : string.Empty,
            i.QuantityOrdered,
            i.QuantityReceived,
            i.UnitCost,
            i.TotalCost
        )).ToList()
    );
}