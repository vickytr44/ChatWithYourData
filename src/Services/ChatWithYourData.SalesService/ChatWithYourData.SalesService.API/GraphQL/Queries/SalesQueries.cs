using ChatWithYourData.SalesService.API.GraphQL.DataLoaders;
using ChatWithYourData.SalesService.Domain.Entities;
using ChatWithYourData.SalesService.Infrastructure.Persistence;
using GreenDonut.Data;
using HotChocolate.Data;
using HotChocolate.Types;
using HotChocolate.Types.Pagination;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.SalesService.API.GraphQL.Queries;

[QueryType]
internal static partial class SalesQueries
{
    [UseFiltering]
    [UseSorting]
    public static async Task<PageConnection<Customer>> GetCustomersAsync(
        PagingArguments pagingArguments,
        QueryContext<Customer> query,
        SalesDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.Customers
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id)
            .With(query)
            .ToPageAsync(pagingArguments, cancellationToken);

    [Lookup]
    public static async Task<Customer?> GetCustomerByIdAsync(
        Guid id,
        QueryContext<Customer> query,
        CustomerByIdDataLoader customerById,
        CancellationToken cancellationToken)
        => await customerById.With(query).LoadAsync(id, cancellationToken);

    [UseFiltering]
    [UseSorting]
    public static async Task<PageConnection<SalesOrder>> GetSalesOrdersAsync(
        PagingArguments pagingArguments,
        QueryContext<SalesOrder> query,
        SalesDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.SalesOrders
            .AsNoTracking()
            .OrderByDescending(o => o.OrderDateUtc)
            .ThenBy(o => o.Id)
            .With(query)
            .ToPageAsync(pagingArguments, cancellationToken);

    [Lookup]
    public static async Task<SalesOrder?> GetSalesOrderByIdAsync(
        Guid id,
        QueryContext<SalesOrder> query,
        SalesOrderByIdDataLoader salesOrderById,
        CancellationToken cancellationToken)
        => await salesOrderById.With(query).LoadAsync(id, cancellationToken);

    [UseFiltering]
    [UseSorting]
    public static async Task<PageConnection<Shipment>> GetShipmentsAsync(
        PagingArguments pagingArguments,
        QueryContext<Shipment> query,
        SalesDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.Shipments
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAtUtc)
            .ThenBy(s => s.Id)
            .With(query)
            .ToPageAsync(pagingArguments, cancellationToken);

    [Lookup]
    public static async Task<Shipment?> GetShipmentByIdAsync(
        Guid id,
        QueryContext<Shipment> query,
        ShipmentByIdDataLoader shipmentById,
        CancellationToken cancellationToken)
        => await shipmentById.With(query).LoadAsync(id, cancellationToken);
}
