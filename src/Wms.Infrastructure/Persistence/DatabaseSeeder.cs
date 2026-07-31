using Microsoft.EntityFrameworkCore;
using Wms.Domain.Entities;
using Wms.Domain.Enums;

namespace Wms.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Якщо в базі вже є товари — не дублюємо дані
        if (await context.Products.AnyAsync())
            return;

        // 1. Створюємо категорії
        var categoryElectronics = new Category { Name = "Електроніка", Description = "Гаджети та комп'ютерна техніка" };
        var categoryFurniture = new Category { Name = "Меблі", Description = "Офісні та складські меблі" };

        await context.Categories.AddRangeAsync(categoryElectronics, categoryFurniture);

        // 2. Створюємо товари
        var productLaptop = new Product
        {
            Name = "Ноутбук Dell XPS 15",
            Sku = "LAP-DELL-001",
            Barcode = "4820000000011",
            Category = categoryElectronics
        };

        var productMonitor = new Product
        {
            Name = "Монітор LG 27'",
            Sku = "MON-LG-002",
            Barcode = "4820000000028",
            Category = categoryElectronics
        };

        var productChair = new Product
        {
            Name = "Офісне крісло Ergonomic",
            Sku = "CHR-ERG-003",
            Barcode = "4820000000035",
            Category = categoryFurniture
        };

        await context.Products.AddRangeAsync(productLaptop, productMonitor, productChair);

        // 3. Створюємо склади
        var mainWarehouse = new Warehouse
        {
            Name = "Центральний склад (Київ)",
            Code = "WH-KIEV-01"
        };

        var regionalWarehouse = new Warehouse
        {
            Name = "Регіональний склад (Львів)",
            Code = "WH-LVIV-01"
        };

        await context.Warehouses.AddRangeAsync(mainWarehouse, regionalWarehouse);

        // 4. Створюємо постачальника та клієнта
        var supplier = new Supplier
        {
            Name = "ТОВ 'ТехноПостач'",
            ContactPerson = "Олексій Іванов",
            Email = "info@techno.ua",
            Phone = "+380441112233",
            Address = "м. Київ, вул. Заводська, 1"
        };

        var customer = new Customer
        {
            Name = "ТОВ 'IT Сервіс Груп'",
            ContactPerson = "Марія Петрова",
            Email = "purchase@itservice.ua",
            Phone = "+380509998877",
            ShippingAddress = "м. Київ, вул. Хрещатик, 15"
        };

        await context.Suppliers.AddAsync(supplier);
        await context.Customers.AddAsync(customer);

        // 5. Заповнюємо початкові залишки на складах (Inventory)
        var inventoryItems = new List<InventoryItem>
        {
            new() { Warehouse = mainWarehouse, Product = productLaptop, QuantityOnHand = 25, QuantityAllocated = 0, Zone = "A", Aisle = "01", Rack = "01", Shelf = "01" },
            new() { Warehouse = mainWarehouse, Product = productMonitor, QuantityOnHand = 5, QuantityAllocated = 0, Zone = "A", Aisle = "01", Rack = "02", Shelf = "01" },
            new() { Warehouse = mainWarehouse, Product = productChair, QuantityOnHand = 40, QuantityAllocated = 0, Zone = "B", Aisle = "02", Rack = "01", Shelf = "01" },
            new() { Warehouse = regionalWarehouse, Product = productLaptop, QuantityOnHand = 10, QuantityAllocated = 0, Zone = "A", Aisle = "01", Rack = "01", Shelf = "01" }
        };

        await context.InventoryItems.AddRangeAsync(inventoryItems);

        // 6. Додаємо початкові записи руху (Stock Movements)
        var movements = new List<StockMovement>
        {
            new() { Product = productLaptop, DestinationWarehouse = mainWarehouse, Quantity = 25, MovementType = MovementType.Inbound, Reason = "Початкове оприбуткування" },
            new() { Product = productMonitor, DestinationWarehouse = mainWarehouse, Quantity = 5, MovementType = MovementType.Inbound, Reason = "Початкове оприбуткування" },
            new() { Product = productChair, DestinationWarehouse = mainWarehouse, Quantity = 40, MovementType = MovementType.Inbound, Reason = "Початкове оприбуткування" }
        };

        await context.StockMovements.AddRangeAsync(movements);

        // Зберігаємо всі засівані дані у базі
        await context.SaveChangesAsync();
    }
}