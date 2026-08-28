using ChatWithYourData.ProcurementService.API.GraphQL.DataLoaders;
using ChatWithYourData.ProcurementService.Domain.Entities;
using ChatWithYourData.ProcurementService.Infrastructure.Persistence;
using GreenDonut.Data;
using HotChocolate.Data;
using HotChocolate.Types;
using HotChocolate.Types.Pagination;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.ProcurementService.API.GraphQL.Queries;

[QueryType]
internal static partial class ProcurementQueries
{
    [UseFiltering]
    [UseSorting]
    public static async Task<PageConnection<Vendor>> GetVendorsAsync(
        PagingArguments pagingArguments,
        QueryContext<Vendor> query,
        ProcurementDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.Vendors
            .AsNoTracking()
            .OrderBy(v => v.Name)
            .ThenBy(v => v.Id)
            .With(query)
            .ToPageAsync(pagingArguments, cancellationToken);

    [Lookup]
    public static async Task<Vendor?> GetVendorByIdAsync(
        Guid id,
        QueryContext<Vendor> query,
        VendorByIdDataLoader vendorById,
        CancellationToken cancellationToken)
        => await vendorById.With(query).LoadAsync(id, cancellationToken);

    [UseFiltering]
    [UseSorting]
    public static async Task<PageConnection<PurchaseOrder>> GetPurchaseOrdersAsync(
        PagingArguments pagingArguments,
        QueryContext<PurchaseOrder> query,
        ProcurementDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.PurchaseOrders
            .AsNoTracking()
            .OrderByDescending(p => p.OrderDateUtc)
            .ThenBy(p => p.Id)
            .With(query)
            .ToPageAsync(pagingArguments, cancellationToken);

    [Lookup]
    public static async Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(
        Guid id,
        QueryContext<PurchaseOrder> query,
        PurchaseOrderByIdDataLoader purchaseOrderById,
        CancellationToken cancellationToken)
        => await purchaseOrderById.With(query).LoadAsync(id, cancellationToken);

    [UseFiltering]
    [UseSorting]
    public static async Task<PageConnection<GoodsReceipt>> GetGoodsReceiptsAsync(
        PagingArguments pagingArguments,
        QueryContext<GoodsReceipt> query,
        ProcurementDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.GoodsReceipts
            .AsNoTracking()
            .OrderByDescending(g => g.ReceivedDateUtc)
            .ThenBy(g => g.Id)
            .With(query)
            .ToPageAsync(pagingArguments, cancellationToken);

    [Lookup]
    public static async Task<GoodsReceipt?> GetGoodsReceiptByIdAsync(
        Guid id,
        QueryContext<GoodsReceipt> query,
        GoodsReceiptByIdDataLoader goodsReceiptById,
        CancellationToken cancellationToken)
        => await goodsReceiptById.With(query).LoadAsync(id, cancellationToken);
}
