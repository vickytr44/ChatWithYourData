using ChatWithYourData.InventoryService.Domain.Entities;
using ChatWithYourData.InventoryService.Infrastructure.Persistence;
using GreenDonut;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.InventoryService.API.GraphQL.DataLoaders;

public class CategoryByIdDataLoader(
    InventoryDbContext dbContext,
    IBatchScheduler batchScheduler,
    DataLoaderOptions? options = null)
    : BatchDataLoader<Guid, Category>(batchScheduler, options ?? new DataLoaderOptions())
{
    protected override async Task<IReadOnlyDictionary<Guid, Category>> LoadBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .Where(c => keys.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);
    }
}

public class WarehouseByIdDataLoader(
    InventoryDbContext dbContext,
    IBatchScheduler batchScheduler,
    DataLoaderOptions? options = null)
    : BatchDataLoader<Guid, Warehouse>(batchScheduler, options ?? new DataLoaderOptions())
{
    protected override async Task<IReadOnlyDictionary<Guid, Warehouse>> LoadBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken)
    {
        return await dbContext.Warehouses
            .AsNoTracking()
            .Where(w => keys.Contains(w.Id))
            .ToDictionaryAsync(w => w.Id, cancellationToken);
    }
}

public class ProductByIdDataLoader(
    InventoryDbContext dbContext,
    IBatchScheduler batchScheduler,
    DataLoaderOptions? options = null)
    : BatchDataLoader<Guid, Product>(batchScheduler, options ?? new DataLoaderOptions())
{
    protected override async Task<IReadOnlyDictionary<Guid, Product>> LoadBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Where(p => keys.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);
    }
}
