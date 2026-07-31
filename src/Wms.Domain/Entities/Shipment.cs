using Wms.Domain.Common;
using Wms.Domain.Enums;

namespace Wms.Domain.Entities;

public class Shipment : BaseEntity
{
    public string TrackingNumber { get; set; } = string.Empty;
    public Guid SalesOrderId { get; set; }
    public SalesOrder SalesOrder { get; set; } = null!;

    public string Carrier { get; set; } = string.Empty;
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;
    public DateTime? ShippedAtUtc { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }
}