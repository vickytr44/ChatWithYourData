using ChatWithYourData.ProcurementService.API.GraphQL.DataLoaders;
using ChatWithYourData.ProcurementService.Domain.Entities;
using ChatWithYourData.ProcurementService.Infrastructure.Persistence;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.ProcurementService.API.GraphQL.Queries;

[GraphQLName("Query")]
public class ProcurementQueries
{
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Vendor> GetVendors(ProcurementDbContext dbContext)
    {
        return dbContext.Vendors.AsNoTracking();
    }

    [UseProjection]
    public async Task<Vendor?> GetVendorByIdAsync(
        Guid id,
        VendorByIdDataLoader dataLoader,
        CancellationToken cancellationToken)
    {
        return await dataLoader.LoadAsync(id, cancellationToken);
    }

    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<PurchaseOrder> GetPurchaseOrders(ProcurementDbContext dbContext)
    {
        return dbContext.PurchaseOrders.AsNoTracking();
    }

    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<GoodsReceipt> GetGoodsReceipts(ProcurementDbContext dbContext)
    {
        return dbContext.GoodsReceipts.AsNoTracking();
    }
}
