using ChatWithYourData.ProcurementService.Domain.Entities;
using ChatWithYourData.ProcurementService.Infrastructure.Persistence;
using GreenDonut.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.ProcurementService.API.GraphQL.DataLoaders;

internal static class ProcurementDataLoaders
{
    [DataLoader]
    public static async Task<Dictionary<Guid, Vendor>> GetVendorByIdAsync(
        IReadOnlyList<Guid> ids,
        QueryContext<Vendor> query,
        ProcurementDbContext context,
        CancellationToken cancellationToken)
        => await context.Vendors
            .AsNoTracking()
            .Where(v => ids.Contains(v.Id))
            .With(query.Include(v => v.Id))
            .ToDictionaryAsync(v => v.Id, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<Guid, PurchaseOrder>> GetPurchaseOrderByIdAsync(
        IReadOnlyList<Guid> ids,
        QueryContext<PurchaseOrder> query,
        ProcurementDbContext context,
        CancellationToken cancellationToken)
        => await context.PurchaseOrders
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .With(query.Include(p => p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<Guid, GoodsReceipt>> GetGoodsReceiptByIdAsync(
        IReadOnlyList<Guid> ids,
        QueryContext<GoodsReceipt> query,
        ProcurementDbContext context,
        CancellationToken cancellationToken)
        => await context.GoodsReceipts
            .AsNoTracking()
            .Where(g => ids.Contains(g.Id))
            .With(query.Include(g => g.Id))
            .ToDictionaryAsync(g => g.Id, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<Guid, List<PurchaseOrderLine>>> GetPoLinesByPoIdAsync(
        IReadOnlyList<Guid> poIds,
        QueryContext<PurchaseOrderLine> query,
        ProcurementDbContext context,
        CancellationToken cancellationToken)
        => (await context.PurchaseOrderLines
            .AsNoTracking()
            .Where(l => poIds.Contains(l.PurchaseOrderId))
            .With(query.Include(l => l.PurchaseOrderId))
            .ToListAsync(cancellationToken))
            .GroupBy(l => l.PurchaseOrderId)
            .ToDictionary(g => g.Key, g => g.ToList());
}
