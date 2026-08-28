using ChatWithYourData.SalesService.Domain.Entities;
using HotChocolate.Data.Filters;

namespace ChatWithYourData.SalesService.API.GraphQL.Filtering;

public sealed class CustomerFilterInputType : FilterInputType<Customer>
{
    protected override void Configure(IFilterInputTypeDescriptor<Customer> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.Name);
        descriptor.Field(t => t.CustomerNumber);
        descriptor.Field(t => t.Email);
        descriptor.Field(t => t.IsActive);
    }
}

public sealed class SalesOrderFilterInputType : FilterInputType<SalesOrder>
{
    protected override void Configure(IFilterInputTypeDescriptor<SalesOrder> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.OrderNumber);
        descriptor.Field(t => t.CustomerId);
        descriptor.Field(t => t.Status);
        descriptor.Field(t => t.OrderDateUtc);
    }
}

public sealed class ShipmentFilterInputType : FilterInputType<Shipment>
{
    protected override void Configure(IFilterInputTypeDescriptor<Shipment> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.SalesOrderId);
        descriptor.Field(t => t.TrackingNumber);
        descriptor.Field(t => t.Status);
    }
}
