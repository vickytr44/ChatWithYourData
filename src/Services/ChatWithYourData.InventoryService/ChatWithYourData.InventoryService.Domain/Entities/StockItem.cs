using ChatWithYourData.InventoryService.Domain.Common;

namespace ChatWithYourData.InventoryService.Domain.Entities;

public class StockItem : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public int ReorderPoint { get; set; } = 10;

    public Product? Product { get; set; }
    public Warehouse? Warehouse { get; set; }
}
