using ChatWithYourData.ProcurementService.Domain.Entities;
using HotChocolate.Data.Filters;

namespace ChatWithYourData.ProcurementService.API.GraphQL.Filtering;

public sealed class VendorFilterInputType : FilterInputType<Vendor>
{
    protected override void Configure(IFilterInputTypeDescriptor<Vendor> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.Name);
        descriptor.Field(t => t.VendorCode);
        descriptor.Field(t => t.ContactEmail);
        descriptor.Field(t => t.IsActive);
    }
}

public sealed class PurchaseOrderFilterInputType : FilterInputType<PurchaseOrder>
{
    protected override void Configure(IFilterInputTypeDescriptor<PurchaseOrder> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.PoNumber);
        descriptor.Field(t => t.VendorId);
        descriptor.Field(t => t.Status);
        descriptor.Field(t => t.OrderDateUtc);
    }
}

public sealed class GoodsReceiptFilterInputType : FilterInputType<GoodsReceipt>
{
    protected override void Configure(IFilterInputTypeDescriptor<GoodsReceipt> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.PurchaseOrderId);
        descriptor.Field(t => t.ReceiptNumber);
        descriptor.Field(t => t.ReceivedDateUtc);
    }
}
