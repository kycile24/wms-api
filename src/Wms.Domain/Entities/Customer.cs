using Wms.Domain.Common;

namespace Wms.Domain.Entities;

public class Customer : BaseEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
}