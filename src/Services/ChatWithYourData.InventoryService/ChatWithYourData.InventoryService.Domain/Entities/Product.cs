using ChatWithYourData.InventoryService.Domain.Common;

namespace ChatWithYourData.InventoryService.Domain.Entities;

public class Product : BaseEntity
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string UnitOfMeasure { get; set; } = "Units";
    public bool IsActive { get; set; } = true;
    public Guid CategoryId { get; set; }

    public Category? Category { get; set; }
    public ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
    public ICollection<StockAdjustment> StockAdjustments { get; set; } = new List<StockAdjustment>();
}
