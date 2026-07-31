using Wms.Domain.Common;
using Wms.Domain.Enums;

namespace Wms.Domain.Entities;

public class PurchaseOrder : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;

    public Guid DestinationWarehouseId { get; set; }
    public Warehouse DestinationWarehouse { get; set; } = null!;

    public OrderStatus Status { get; set; } = OrderStatus.Draft;
    public decimal TotalAmount { get; set; }
    public DateTime OrderDateUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDeliveryDateUtc { get; set; }

    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}