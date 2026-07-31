using Microsoft.EntityFrameworkCore;
using Wms.Application.DTOs.SalesOrders;
using Wms.Application.Interfaces;
using Wms.Domain.Common;
using Wms.Domain.Entities;
using Wms.Domain.Enums;

namespace Wms.Application.Services;

public class SalesOrderService : ISalesOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public SalesOrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SalesOrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Repository<SalesOrder>()
            .Query()
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.SourceWarehouse)
            .Include(s => s.Shipments)
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (order is null)
            return Result<SalesOrderDto>.Failure(new Error("SalesOrder.NotFound", $"Замовлення з ID '{id}' не знайдено."));

        return Result<SalesOrderDto>.Success(MapToDto(order));
    }

    public async Task<Result<IReadOnlyList<SalesOrderDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Repository<SalesOrder>()
            .Query()
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.SourceWarehouse)
            .Include(s => s.Shipments)
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .OrderByDescending(s => s.OrderDateUtc)
            .Select(s => MapToDto(s))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<SalesOrderDto>>.Success(orders);
    }

    public async Task<Result<Guid>> CreateAsync(CreateSalesOrderDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Items is null || !dto.Items.Any())
            return Result<Guid>.Failure(new Error("SalesOrder.EmptyItems", "Замовлення повинно містити хоча б один товар."));

        var customerExists = await _unitOfWork.Repository<Customer>().ExistsAsync(c => c.Id == dto.CustomerId, cancellationToken);
        if (!customerExists)
            return Result<Guid>.Failure(new Error("Customer.NotFound", "Вказаного клієнта не знайдено."));

        var warehouseExists = await _unitOfWork.Repository<Warehouse>().ExistsAsync(w => w.Id == dto.SourceWarehouseId, cancellationToken);
        if (!warehouseExists)
            return Result<Guid>.Failure(new Error("Warehouse.NotFound", "Вказаного склада відвантаження не знайдено."));

        var inventoryRepo = _unitOfWork.Repository<InventoryItem>();

        // 1. Перевіряємо та резервуємо товар (QuantityAllocated)
        foreach (var itemDto in dto.Items)
        {
            var inventory = await inventoryRepo.Query()
                .FirstOrDefaultAsync(i => i.WarehouseId == dto.SourceWarehouseId && i.ProductId == itemDto.ProductId, cancellationToken);

            if (inventory is null || inventory.QuantityAvailable < itemDto.QuantityOrdered)
            {
                return Result<Guid>.Failure(new Error("SalesOrder.InsufficientStock",
                    $"Недостатньо доступного товару (ID: {itemDto.ProductId}) на складі. Доступно: {inventory?.QuantityAvailable ?? 0}."));
            }

            // Резервуємо залишки
            inventory.QuantityAllocated += itemDto.QuantityOrdered;
            inventoryRepo.Update(inventory);
        }

        // 2. Створюємо замовлення
        var order = new SalesOrder
        {
            OrderNumber = dto.OrderNumber.Trim(),
            CustomerId = dto.CustomerId,
            SourceWarehouseId = dto.SourceWarehouseId,
            Status = OrderStatus.Processing,
            OrderDateUtc = DateTime.UtcNow,
            TotalAmount = dto.Items.Sum(i => i.QuantityOrdered * i.UnitPrice)
        };

        foreach (var itemDto in dto.Items)
        {
            order.Items.Add(new SalesOrderItem
            {
                ProductId = itemDto.ProductId,
                QuantityOrdered = itemDto.QuantityOrdered,
                QuantityShipped = 0,
                UnitPrice = itemDto.UnitPrice
            });
        }

        await _unitOfWork.Repository<SalesOrder>().AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(order.Id);
    }

    public async Task<Result> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<SalesOrder>();
        var order = await repo.GetByIdAsync(id, cancellationToken);

        if (order is null)
            return Result.Failure(new Error("SalesOrder.NotFound", $"Замовлення з ID '{id}' не знайдено."));

        order.Status = status;
        repo.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<Guid>> ShipOrderAsync(Guid orderId, CreateShipmentDto dto, CancellationToken cancellationToken = default)
    {
        var orderRepo = _unitOfWork.Repository<SalesOrder>();
        var order = await orderRepo.Query()
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == orderId, cancellationToken);

        if (order is null)
            return Result<Guid>.Failure(new Error("SalesOrder.NotFound", $"Замовлення з ID '{orderId}' не знайдено."));

        if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
            return Result<Guid>.Failure(new Error("SalesOrder.InvalidStatus", "Неможливо відвантажити завершене або скасоване замовлення."));

        var inventoryRepo = _unitOfWork.Repository<InventoryItem>();
        var movementRepo = _unitOfWork.Repository<StockMovement>();

        // Списання залишків та зняття резерву
        foreach (var item in order.Items)
        {
            var qtyToShip = item.QuantityOrdered - item.QuantityShipped;
            if (qtyToShip <= 0) continue;

            var inventory = await inventoryRepo.Query()
                .FirstOrDefaultAsync(i => i.WarehouseId == order.SourceWarehouseId && i.ProductId == item.ProductId, cancellationToken);

            if (inventory is null)
                return Result<Guid>.Failure(new Error("Inventory.NotFound", "Товар не знайдено на складі."));

            inventory.QuantityOnHand -= qtyToShip;
            inventory.QuantityAllocated -= qtyToShip;
            if (inventory.QuantityAllocated < 0) inventory.QuantityAllocated = 0;

            inventoryRepo.Update(inventory);
            item.QuantityShipped += qtyToShip;

            // Запис руху (Outbound)
            var movement = new StockMovement
            {
                ProductId = item.ProductId,
                SourceWarehouseId = order.SourceWarehouseId,
                Quantity = qtyToShip,
                MovementType = MovementType.Outbound,
                ReferenceNumber = order.OrderNumber,
                Reason = $"Відвантаження за замовленням #{order.OrderNumber}"
            };
            await movementRepo.AddAsync(movement, cancellationToken);
        }

        // Створення ТТН / Відвантаження
        var shipment = new Shipment
        {
            SalesOrderId = order.Id,
            TrackingNumber = dto.TrackingNumber.Trim(),
            Carrier = dto.Carrier.Trim(),
            Status = ShipmentStatus.Shipped,
            ShippedAtUtc = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Shipment>().AddAsync(shipment, cancellationToken);

        order.Status = OrderStatus.Completed;
        orderRepo.Update(order);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(shipment.Id);
    }

    public async Task<Result> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var orderRepo = _unitOfWork.Repository<SalesOrder>();
        var order = await orderRepo.Query()
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == orderId, cancellationToken);

        if (order is null)
            return Result.Failure(new Error("SalesOrder.NotFound", $"Замовлення з ID '{orderId}' не знайдено."));

        if (order.Status == OrderStatus.Completed)
            return Result.Failure(new Error("SalesOrder.CannotCancel", "Завершене замовлення не можна скасувати."));

        var inventoryRepo = _unitOfWork.Repository<InventoryItem>();

        // Знімаємо резерв залишків
        foreach (var item in order.Items)
        {
            var qtyAllocated = item.QuantityOrdered - item.QuantityShipped;
            if (qtyAllocated <= 0) continue;

            var inventory = await inventoryRepo.Query()
                .FirstOrDefaultAsync(i => i.WarehouseId == order.SourceWarehouseId && i.ProductId == item.ProductId, cancellationToken);

            if (inventory is not null)
            {
                inventory.QuantityAllocated -= qtyAllocated;
                if (inventory.QuantityAllocated < 0) inventory.QuantityAllocated = 0;
                inventoryRepo.Update(inventory);
            }
        }

        order.Status = OrderStatus.Cancelled;
        orderRepo.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static SalesOrderDto MapToDto(SalesOrder s) => new(
        s.Id,
        s.OrderNumber,
        s.CustomerId,
        s.Customer != null ? s.Customer.Name : string.Empty,
        s.SourceWarehouseId,
        s.SourceWarehouse != null ? s.SourceWarehouse.Name : string.Empty,
        s.Status,
        s.TotalAmount,
        s.OrderDateUtc,
        s.Items.Select(i => new SalesOrderItemDto(
            i.Id,
            i.ProductId,
            i.Product != null ? i.Product.Name : string.Empty,
            i.Product != null ? i.Product.Sku : string.Empty,
            i.QuantityOrdered,
            i.QuantityShipped,
            i.UnitPrice,
            i.TotalPrice
        )).ToList(),
        s.Shipments.Select(sh => new ShipmentDto(
            sh.Id,
            sh.TrackingNumber,
            sh.Carrier,
            sh.Status,
            sh.ShippedAtUtc,
            sh.DeliveredAtUtc
        )).ToList()
    );
}