using Wms.Domain.Common;

namespace Wms.Domain.Entities;

public class Product : BaseEntity, ISoftDelete
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Barcode { get; set; }
    public decimal UnitPrice { get; set; }
    public string UnitOfMeasure { get; set; } = "PCS";
    public int MinimumStockThreshold { get; set; } = 10;
    public string? ImageUrl { get; set; }

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}