using ChatWithYourData.InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.InventoryService.Application.Common.Interfaces;

public interface IInventoryDbContext
{
    DbSet<Product> Products { get; }
    DbSet<Category> Categories { get; }
    DbSet<Warehouse> Warehouses { get; }
    DbSet<StockItem> StockItems { get; }
    DbSet<StockAdjustment> StockAdjustments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
