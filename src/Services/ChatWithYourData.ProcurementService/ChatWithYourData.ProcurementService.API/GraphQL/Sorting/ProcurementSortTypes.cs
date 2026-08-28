using ChatWithYourData.ProcurementService.Domain.Entities;
using HotChocolate.Data.Sorting;

namespace ChatWithYourData.ProcurementService.API.GraphQL.Sorting;

public sealed class VendorSortInputType : SortInputType<Vendor>
{
    protected override void Configure(ISortInputTypeDescriptor<Vendor> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.Name);
        descriptor.Field(t => t.VendorCode);
        descriptor.Field(t => t.CreatedAtUtc);
    }
}

public sealed class PurchaseOrderSortInputType : SortInputType<PurchaseOrder>
{
    protected override void Configure(ISortInputTypeDescriptor<PurchaseOrder> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.PoNumber);
        descriptor.Field(t => t.OrderDateUtc);
        descriptor.Field(t => t.TotalCost);
    }
}

public sealed class GoodsReceiptSortInputType : SortInputType<GoodsReceipt>
{
    protected override void Configure(ISortInputTypeDescriptor<GoodsReceipt> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.ReceiptNumber);
        descriptor.Field(t => t.ReceivedDateUtc);
    }
}
