using Wms.Domain.Common;
using Wms.Domain.Enums;

namespace Wms.Domain.Entities;

public class StockMovement : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid? SourceWarehouseId { get; set; }
    public Warehouse? SourceWarehouse { get; set; }

    public Guid? DestinationWarehouseId { get; set; }
    public Warehouse? DestinationWarehouse { get; set; }

    public int Quantity { get; set; }
    public MovementType MovementType { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}