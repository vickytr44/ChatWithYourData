using ChatWithYourData.SalesService.Domain.Entities;
using ChatWithYourData.SalesService.Infrastructure.Persistence;
using GreenDonut.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.SalesService.API.GraphQL.DataLoaders;

internal static class SalesDataLoaders
{
    [DataLoader]
    public static async Task<Dictionary<Guid, Customer>> GetCustomerByIdAsync(
        IReadOnlyList<Guid> ids,
        QueryContext<Customer> query,
        SalesDbContext context,
        CancellationToken cancellationToken)
        => await context.Customers
            .AsNoTracking()
            .Where(c => ids.Contains(c.Id))
            .With(query.Include(c => c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<Guid, SalesOrder>> GetSalesOrderByIdAsync(
        IReadOnlyList<Guid> ids,
        QueryContext<SalesOrder> query,
        SalesDbContext context,
        CancellationToken cancellationToken)
        => await context.SalesOrders
            .AsNoTracking()
            .Where(o => ids.Contains(o.Id))
            .With(query.Include(o => o.Id))
            .ToDictionaryAsync(o => o.Id, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<Guid, Shipment>> GetShipmentByIdAsync(
        IReadOnlyList<Guid> ids,
        QueryContext<Shipment> query,
        SalesDbContext context,
        CancellationToken cancellationToken)
        => await context.Shipments
            .AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .With(query.Include(s => s.Id))
            .ToDictionaryAsync(s => s.Id, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<Guid, List<SalesOrderLine>>> GetOrderLinesByOrderIdAsync(
        IReadOnlyList<Guid> orderIds,
        QueryContext<SalesOrderLine> query,
        SalesDbContext context,
        CancellationToken cancellationToken)
        => (await context.SalesOrderLines
            .AsNoTracking()
            .Where(l => orderIds.Contains(l.SalesOrderId))
            .With(query.Include(l => l.SalesOrderId))
            .ToListAsync(cancellationToken))
            .GroupBy(l => l.SalesOrderId)
            .ToDictionary(g => g.Key, g => g.ToList());
}
