using ChatWithYourData.InventoryService.API.GraphQL.DataLoaders;
using ChatWithYourData.InventoryService.Domain.Entities;
using ChatWithYourData.InventoryService.Infrastructure.Persistence;
using GreenDonut.Data;
using HotChocolate.Data;
using HotChocolate.Types;
using HotChocolate.Types.Pagination;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.InventoryService.API.GraphQL.Queries;

[QueryType]
internal static partial class InventoryQueries
{
    [UseFiltering]
    [UseSorting]
    public static async Task<PageConnection<Product>> GetProductsAsync(
        PagingArguments pagingArguments,
        QueryContext<Product> query,
        InventoryDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .With(query)
            .ToPageAsync(pagingArguments, cancellationToken);

    [Lookup]
    public static async Task<Product?> GetProductByIdAsync(
        Guid id,
        QueryContext<Product> query,
        ProductByIdDataLoader productById,
        CancellationToken cancellationToken)
        => await productById.With(query).LoadAsync(id, cancellationToken);

    [UseFiltering]
    [UseSorting]
    public static async Task<PageConnection<Category>> GetCategoriesAsync(
        PagingArguments pagingArguments,
        QueryContext<Category> query,
        InventoryDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id)
            .With(query)
            .ToPageAsync(pagingArguments, cancellationToken);

    [Lookup]
    public static async Task<Category?> GetCategoryByIdAsync(
        Guid id,
        QueryContext<Category> query,
        CategoryByIdDataLoader categoryById,
        CancellationToken cancellationToken)
        => await categoryById.With(query).LoadAsync(id, cancellationToken);

    [UseFiltering]
    [UseSorting]
    public static async Task<PageConnection<Warehouse>> GetWarehousesAsync(
        PagingArguments pagingArguments,
        QueryContext<Warehouse> query,
        InventoryDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.Warehouses
            .AsNoTracking()
            .OrderBy(w => w.Name)
            .ThenBy(w => w.Id)
            .With(query)
            .ToPageAsync(pagingArguments, cancellationToken);

    [UseFiltering]
    [UseSorting]
    public static async Task<PageConnection<StockItem>> GetStockItemsAsync(
        PagingArguments pagingArguments,
        QueryContext<StockItem> query,
        InventoryDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.StockItems
            .AsNoTracking()
            .OrderBy(s => s.ProductId)
            .ThenBy(s => s.Id)
            .With(query)
            .ToPageAsync(pagingArguments, cancellationToken);

    [Lookup]
    public static async Task<StockItem?> GetStockItemByIdAsync(
        Guid id,
        QueryContext<StockItem> query,
        StockItemByIdDataLoader stockItemById,
        CancellationToken cancellationToken)
        => await stockItemById.With(query).LoadAsync(id, cancellationToken);

    [UseFiltering]
    public static async Task<PageConnection<StockItem>> GetLowStockAlertsAsync(
        PagingArguments pagingArguments,
        QueryContext<StockItem> query,
        InventoryDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.StockItems
            .AsNoTracking()
            .Where(s => s.QuantityOnHand <= s.ReorderPoint)
            .OrderBy(s => s.QuantityOnHand)
            .ThenBy(s => s.Id)
            .With(query)
            .ToPageAsync(pagingArguments, cancellationToken);
}
