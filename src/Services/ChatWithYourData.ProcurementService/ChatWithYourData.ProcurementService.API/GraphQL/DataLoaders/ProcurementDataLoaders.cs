using ChatWithYourData.ProcurementService.Domain.Entities;
using ChatWithYourData.ProcurementService.Infrastructure.Persistence;
using GreenDonut;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.ProcurementService.API.GraphQL.DataLoaders;

public class VendorByIdDataLoader(
    ProcurementDbContext dbContext,
    IBatchScheduler batchScheduler,
    DataLoaderOptions? options = null)
    : BatchDataLoader<Guid, Vendor>(batchScheduler, options ?? new DataLoaderOptions())
{
    protected override async Task<IReadOnlyDictionary<Guid, Vendor>> LoadBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken)
    {
        return await dbContext.Vendors
            .AsNoTracking()
            .Where(v => keys.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, cancellationToken);
    }
}

public class PoLinesByPoIdDataLoader(
    ProcurementDbContext dbContext,
    IBatchScheduler batchScheduler,
    DataLoaderOptions? options = null)
    : BatchDataLoader<Guid, List<PurchaseOrderLine>>(batchScheduler, options ?? new DataLoaderOptions())
{
    protected override async Task<IReadOnlyDictionary<Guid, List<PurchaseOrderLine>>> LoadBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken)
    {
        var lines = await dbContext.PurchaseOrderLines
            .AsNoTracking()
            .Where(l => keys.Contains(l.PurchaseOrderId))
            .ToListAsync(cancellationToken);

        return lines.GroupBy(l => l.PurchaseOrderId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
