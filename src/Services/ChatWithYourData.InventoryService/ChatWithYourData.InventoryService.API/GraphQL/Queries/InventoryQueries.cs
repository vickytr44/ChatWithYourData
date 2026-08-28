using ChatWithYourData.InventoryService.API.GraphQL.DataLoaders;
using ChatWithYourData.InventoryService.Domain.Entities;
using ChatWithYourData.InventoryService.Infrastructure.Persistence;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.InventoryService.API.GraphQL.Queries;

[GraphQLName("Query")]
public class InventoryQueries
{
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Product> GetProducts(InventoryDbContext dbContext)
    {
        return dbContext.Products.AsNoTracking();
    }

    [UseProjection]
    [Lookup]
    public async Task<Product?> GetProductByIdAsync(
        Guid id,
        ProductByIdDataLoader dataLoader,
        CancellationToken cancellationToken)
    {
        return await dataLoader.LoadAsync(id, cancellationToken);
    }

    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Category> GetCategories(InventoryDbContext dbContext)
    {
        return dbContext.Categories.AsNoTracking();
    }

    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Warehouse> GetWarehouses(InventoryDbContext dbContext)
    {
        return dbContext.Warehouses.AsNoTracking();
    }

    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<StockItem> GetStockItems(InventoryDbContext dbContext)
    {
        return dbContext.StockItems.AsNoTracking();
    }

    [UseFiltering]
    public IQueryable<StockItem> GetLowStockAlerts(InventoryDbContext dbContext)
    {
        return dbContext.StockItems
            .AsNoTracking()
            .Where(s => s.QuantityOnHand <= s.ReorderPoint);
    }
}
