using ChatWithYourData.SalesService.API.GraphQL.DataLoaders;
using ChatWithYourData.SalesService.Domain.Entities;
using ChatWithYourData.SalesService.Infrastructure.Persistence;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.SalesService.API.GraphQL.Queries;

public class SalesQueries
{
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Customer> GetCustomers(SalesDbContext dbContext)
    {
        return dbContext.Customers.AsNoTracking();
    }

    [UseProjection]
    public async Task<Customer?> GetCustomerByIdAsync(
        Guid id,
        CustomerByIdDataLoader dataLoader,
        CancellationToken cancellationToken)
    {
        return await dataLoader.LoadAsync(id, cancellationToken);
    }

    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<SalesOrder> GetSalesOrders(SalesDbContext dbContext)
    {
        return dbContext.SalesOrders.AsNoTracking();
    }

    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Shipment> GetShipments(SalesDbContext dbContext)
    {
        return dbContext.Shipments.AsNoTracking();
    }
}
