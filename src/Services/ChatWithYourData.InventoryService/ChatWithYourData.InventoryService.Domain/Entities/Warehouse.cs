using ChatWithYourData.InventoryService.Domain.Common;

namespace ChatWithYourData.InventoryService.Domain.Entities;

public class Warehouse : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LocationAddress { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
}
