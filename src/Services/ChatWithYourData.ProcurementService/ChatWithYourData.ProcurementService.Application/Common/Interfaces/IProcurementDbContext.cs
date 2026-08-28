using ChatWithYourData.ProcurementService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.ProcurementService.Application.Common.Interfaces;

public interface IProcurementDbContext
{
    DbSet<Vendor> Vendors { get; }
    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<PurchaseOrderLine> PurchaseOrderLines { get; }
    DbSet<GoodsReceipt> GoodsReceipts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
