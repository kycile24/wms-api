using Wms.Domain.Common;

namespace Wms.Domain.Entities;

public class SalesOrderItem : BaseEntity
{
    public Guid SalesOrderId { get; set; }
    public SalesOrder SalesOrder { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int QuantityOrdered { get; set; }
    public int QuantityShipped { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => QuantityOrdered * UnitPrice;
}