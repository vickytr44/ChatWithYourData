using ChatWithYourData.SalesService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.SalesService.Application.Common.Interfaces;

public interface ISalesDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<SalesOrder> SalesOrders { get; }
    DbSet<SalesOrderLine> SalesOrderLines { get; }
    DbSet<Shipment> Shipments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
