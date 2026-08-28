using ChatWithYourData.SalesService.Domain.Entities;
using ChatWithYourData.SalesService.Infrastructure.Persistence;
using GreenDonut;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.SalesService.API.GraphQL.DataLoaders;

public class CustomerByIdDataLoader(
    SalesDbContext dbContext,
    IBatchScheduler batchScheduler,
    DataLoaderOptions? options = null)
    : BatchDataLoader<Guid, Customer>(batchScheduler, options ?? new DataLoaderOptions())
{
    protected override async Task<IReadOnlyDictionary<Guid, Customer>> LoadBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken)
    {
        return await dbContext.Customers
            .AsNoTracking()
            .Where(c => keys.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);
    }
}

public class OrderLinesByOrderIdDataLoader(
    SalesDbContext dbContext,
    IBatchScheduler batchScheduler,
    DataLoaderOptions? options = null)
    : BatchDataLoader<Guid, List<SalesOrderLine>>(batchScheduler, options ?? new DataLoaderOptions())
{
    protected override async Task<IReadOnlyDictionary<Guid, List<SalesOrderLine>>> LoadBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken)
    {
        var lines = await dbContext.SalesOrderLines
            .AsNoTracking()
            .Where(l => keys.Contains(l.SalesOrderId))
            .ToListAsync(cancellationToken);

        return lines.GroupBy(l => l.SalesOrderId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
