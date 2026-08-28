using ChatWithYourData.SalesService.Domain.Entities;
using HotChocolate.Data.Sorting;

namespace ChatWithYourData.SalesService.API.GraphQL.Sorting;

public sealed class CustomerSortInputType : SortInputType<Customer>
{
    protected override void Configure(ISortInputTypeDescriptor<Customer> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.Name);
        descriptor.Field(t => t.CustomerNumber);
        descriptor.Field(t => t.CreatedAtUtc);
    }
}

public sealed class SalesOrderSortInputType : SortInputType<SalesOrder>
{
    protected override void Configure(ISortInputTypeDescriptor<SalesOrder> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.OrderNumber);
        descriptor.Field(t => t.OrderDateUtc);
        descriptor.Field(t => t.TotalAmount);
    }
}

public sealed class ShipmentSortInputType : SortInputType<Shipment>
{
    protected override void Configure(ISortInputTypeDescriptor<Shipment> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.TrackingNumber);
        descriptor.Field(t => t.ShippedAtUtc);
    }
}
