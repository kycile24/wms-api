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

        var catalog = new (string Category, string Name, string Sku, decimal Price)[]
        {
            ("Accessories", "Logitech MX Master 3S", "MX3S", 3990m),
            ("Accessories", "Logitech K380", "K380", 1890m),
            ("Accessories", "Anker USB-C Hub", "HUB7", 2150m),
            ("Accessories", "UGREEN HDMI 2 m", "HDMI2", 350m),
            ("Accessories", "Baseus 65 W charger", "GAN65", 1790m),
            ("Accessories", "Jabra Evolve 20", "EV20", 2190m),
            ("Components", "Kingston NV2 1 TB", "NV21T", 2890m),
            ("Components", "Samsung 990 EVO 2 TB", "990E2", 5990m),
            ("Components", "Crucial DDR5 32 GB", "DDR532", 3790m),
            ("Components", "Noctua NF-A12", "NFA12", 1290m),
            ("Components", "Corsair RM750e", "RM750", 4690m),
            ("Networking", "TP-Link Archer AX55", "AX55", 3590m),
            ("Networking", "UniFi U6 Lite", "U6L", 4490m),
            ("Networking", "Netgear GS108", "GS108", 1590m),
            ("Networking", "UGREEN CAT6 5 m", "CAT65", 260m),
            ("Networking", "TP-Link Deco X50", "X50", 3890m),
            ("Office", "Brother HL-L2400D", "HL2400", 6750m),
            ("Office", "Epson EcoTank L3250", "L3250", 8190m),
            ("Office", "Fellowes monitor arm", "ARM1", 1990m),
            ("Office", "Leitz document tray", "TRAY1", 420m),
            ("Office", "A4 copy paper 500 sheets", "A4500", 245m),
            ("Packing", "Kraft carton 40 cm", "BOX40", 58m),
            ("Packing", "Thermal labels 100 x 150", "LBL150", 385m),
            ("Packing", "Packing tape 48 mm", "TAPE48", 68m),
            ("Packing", "Bubble wrap 10 m", "WRAP10", 195m),
            ("Packing", "Zebra ZD220 printer", "ZD220", 7390m),
            ("Tools", "Bosch laser measure", "GLM40", 3290m),
            ("Tools", "Stanley tape measure", "ST5M", 350m),
            ("Tools", "Honeywell barcode scanner", "SCAN1", 2890m),
            ("Tools", "Dymo label maker", "DYMO1", 1690m),
        };
        var categories = catalog.Select(item => item.Category).Distinct()
            .ToDictionary(name => name, name => new Category { Name = name });
        for (int index = 0; index < catalog.Length; index++)
        {
            var item = catalog[index];
            var product = new Product { Sku = item.Sku, Name = item.Name, UnitPrice = item.Price,
                Category = categories[item.Category], MinimumStockThreshold = 6 + index % 8 };
            foreach (var warehouse in new[] { mainWarehouse, regionalWarehouse })
            {
                var quantity = (index * 11 + (warehouse == mainWarehouse ? 3 : 17)) % 54;
                context.InventoryItems.Add(new InventoryItem { Product = product, Warehouse = warehouse,
                    QuantityOnHand = quantity, Zone = "C", Aisle = (1 + index / 10).ToString("00"),
                    Rack = (1 + index % 10).ToString("00"), Shelf = "01" });
                if (quantity > 0)
                    context.StockMovements.Add(new StockMovement { Product = product, DestinationWarehouse = warehouse,
                        MovementType = MovementType.Inbound, Quantity = quantity, Reason = "Opening stock" });
            }
        }

        // Зберігаємо всі засівані дані у базі
        await context.SaveChangesAsync();
    }
}