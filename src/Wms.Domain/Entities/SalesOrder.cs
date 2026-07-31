using Wms.Domain.Common;
using Wms.Domain.Enums;

namespace Wms.Domain.Entities;

public class SalesOrder : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public Guid SourceWarehouseId { get; set; }
    public Warehouse SourceWarehouse { get; set; } = null!;

    public OrderStatus Status { get; set; } = OrderStatus.Draft;
    public decimal TotalAmount { get; set; }
    public DateTime OrderDateUtc { get; set; } = DateTime.UtcNow;

    public ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}