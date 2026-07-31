using Wms.Domain.Common;

namespace Wms.Domain.Entities;

public class InventoryItem : BaseEntity
{
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int QuantityOnHand { get; set; }
    public int QuantityAllocated { get; set; }
    public int QuantityAvailable => QuantityOnHand - QuantityAllocated;

    public string Zone { get; set; } = "Default";
    public string Aisle { get; set; } = "01";
    public string Rack { get; set; } = "01";
    public string Shelf { get; set; } = "01";
}