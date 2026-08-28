using ChatWithYourData.InventoryService.Domain.Entities;
using ChatWithYourData.InventoryService.Infrastructure.Persistence;
using GreenDonut.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.InventoryService.API.GraphQL.DataLoaders;

internal static class InventoryDataLoaders
{
    [DataLoader]
    public static async Task<Dictionary<Guid, Product>> GetProductByIdAsync(
        IReadOnlyList<Guid> ids,
        QueryContext<Product> query,
        InventoryDbContext context,
        CancellationToken cancellationToken)
        => await context.Products
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .With(query.Include(p => p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<Guid, Category>> GetCategoryByIdAsync(
        IReadOnlyList<Guid> ids,
        QueryContext<Category> query,
        InventoryDbContext context,
        CancellationToken cancellationToken)
        => await context.Categories
            .AsNoTracking()
            .Where(c => ids.Contains(c.Id))
            .With(query.Include(c => c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<Guid, Warehouse>> GetWarehouseByIdAsync(
        IReadOnlyList<Guid> ids,
        QueryContext<Warehouse> query,
        InventoryDbContext context,
        CancellationToken cancellationToken)
        => await context.Warehouses
            .AsNoTracking()
            .Where(w => ids.Contains(w.Id))
            .With(query.Include(w => w.Id))
            .ToDictionaryAsync(w => w.Id, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<Guid, StockItem>> GetStockItemByIdAsync(
        IReadOnlyList<Guid> ids,
        QueryContext<StockItem> query,
        InventoryDbContext context,
        CancellationToken cancellationToken)
        => await context.StockItems
            .AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .With(query.Include(s => s.Id))
            .ToDictionaryAsync(s => s.Id, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<Guid, List<StockItem>>> GetStockItemsByProductIdAsync(
        IReadOnlyList<Guid> productIds,
        QueryContext<StockItem> query,
        InventoryDbContext context,
        CancellationToken cancellationToken)
        => (await context.StockItems
            .AsNoTracking()
            .Where(s => productIds.Contains(s.ProductId))
            .With(query.Include(s => s.ProductId))
            .ToListAsync(cancellationToken))
            .GroupBy(s => s.ProductId)
            .ToDictionary(g => g.Key, g => g.ToList());
}
