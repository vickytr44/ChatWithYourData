using ChatWithYourData.InventoryService.Domain.Common;
using ChatWithYourData.InventoryService.Domain.Enums;

namespace ChatWithYourData.InventoryService.Domain.Entities;

public class StockAdjustment : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public int QuantityDelta { get; set; }
    public AdjustmentReason Reason { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime AdjustedAtUtc { get; set; } = DateTime.UtcNow;

    public Product? Product { get; set; }
    public Warehouse? Warehouse { get; set; }
}
