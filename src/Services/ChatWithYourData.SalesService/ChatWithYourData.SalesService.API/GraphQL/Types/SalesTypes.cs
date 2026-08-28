using ChatWithYourData.SalesService.API.GraphQL.DataLoaders;
using ChatWithYourData.SalesService.Domain.Entities;
using HotChocolate.Types;

namespace ChatWithYourData.SalesService.API.GraphQL.Types;

public class SalesOrderType : ObjectType<SalesOrder>
{
    protected override void Configure(IObjectTypeDescriptor<SalesOrder> descriptor)
    {
        descriptor.Description("Represents a customer sales order with line items.");

        descriptor.Field(o => o.CustomerId)
            .IsProjected(true);

        descriptor.Field(o => o.Customer)
            .ResolveWith<SalesOrderResolvers>(r => r.GetCustomerAsync(default!, default!, default!))
            .Description("The customer who placed the order (resolved via DataLoader).");

        descriptor.Field(o => o.Lines)
            .ResolveWith<SalesOrderResolvers>(r => r.GetLinesAsync(default!, default!, default!))
            .Description("The line items for this sales order (resolved via DataLoader).");
    }

    private class SalesOrderResolvers
    {
        public async Task<Customer?> GetCustomerAsync(
            [Parent] SalesOrder order,
            CustomerByIdDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            return await dataLoader.LoadAsync(order.CustomerId, cancellationToken);
        }

        public async Task<List<SalesOrderLine>> GetLinesAsync(
            [Parent] SalesOrder order,
            OrderLinesByOrderIdDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            var lines = await dataLoader.LoadAsync(order.Id, cancellationToken);
            return lines ?? new List<SalesOrderLine>();
        }
    }
}
