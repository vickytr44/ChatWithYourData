using ChatWithYourData.SalesService.API.GraphQL.DataLoaders;
using ChatWithYourData.SalesService.Domain.Entities;
using GreenDonut.Data;
using HotChocolate;
using HotChocolate.Types;

namespace ChatWithYourData.SalesService.API.GraphQL.Types;

[GraphQLName("Product")]
public class ProductEntityStub
{
    public Guid Id { get; set; }
}

[ObjectType<ProductEntityStub>]
internal static partial class ProductEntityStubNode
{
    static partial void Configure(IObjectTypeDescriptor<ProductEntityStub> descriptor)
    {
        descriptor.Field(p => p.Id).Type<NonNullType<IdType>>();
    }
}

[ObjectType<SalesOrder>]
internal static partial class SalesOrderNode
{
    [BindMember(nameof(SalesOrder.CustomerId))]
    public static async Task<Customer?> GetCustomerAsync(
        [Parent(requires: nameof(SalesOrder.CustomerId))] SalesOrder order,
        QueryContext<Customer> query,
        CustomerByIdDataLoader customerById,
        CancellationToken cancellationToken)
        => await customerById.With(query).LoadAsync(order.CustomerId, cancellationToken);

    public static async Task<List<SalesOrderLine>> GetLinesAsync(
        [Parent(requires: nameof(SalesOrder.Id))] SalesOrder order,
        QueryContext<SalesOrderLine> query,
        OrderLinesByOrderIdDataLoader orderLinesByOrderId,
        CancellationToken cancellationToken)
        => await orderLinesByOrderId.With(query).LoadAsync(order.Id, cancellationToken) ?? [];

    static partial void Configure(IObjectTypeDescriptor<SalesOrder> descriptor)
    {
        descriptor.Ignore(o => o.Customer);
        descriptor.Ignore(o => o.Lines);
        descriptor.Ignore(o => o.Shipment);
    }
}

[ObjectType<Customer>]
internal static partial class CustomerNode
{
    static partial void Configure(IObjectTypeDescriptor<Customer> descriptor)
    {
        descriptor.Ignore(c => c.SalesOrders);
    }
}

[ObjectType<SalesOrderLine>]
internal static partial class SalesOrderLineNode
{
    [BindMember(nameof(SalesOrderLine.ProductId))]
    public static ProductEntityStub GetProduct(
        [Parent(requires: nameof(SalesOrderLine.ProductId))] SalesOrderLine line)
        => new ProductEntityStub { Id = line.ProductId };

    static partial void Configure(IObjectTypeDescriptor<SalesOrderLine> descriptor)
    {
        descriptor.Ignore(l => l.SalesOrder);
    }
}

[ObjectType<Shipment>]
internal static partial class ShipmentNode
{
    [BindMember(nameof(Shipment.SalesOrderId))]
    public static async Task<SalesOrder?> GetSalesOrderAsync(
        [Parent(requires: nameof(Shipment.SalesOrderId))] Shipment shipment,
        QueryContext<SalesOrder> query,
        SalesOrderByIdDataLoader salesOrderById,
        CancellationToken cancellationToken)
        => await salesOrderById.With(query).LoadAsync(shipment.SalesOrderId, cancellationToken);

    static partial void Configure(IObjectTypeDescriptor<Shipment> descriptor)
    {
        descriptor.Ignore(s => s.SalesOrder);
    }
}
