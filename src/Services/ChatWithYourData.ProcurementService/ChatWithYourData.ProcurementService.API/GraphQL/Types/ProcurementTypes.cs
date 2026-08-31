using ChatWithYourData.ProcurementService.API.GraphQL.DataLoaders;
using ChatWithYourData.ProcurementService.Domain.Entities;
using GreenDonut.Data;
using HotChocolate;
using HotChocolate.Types;

namespace ChatWithYourData.ProcurementService.API.GraphQL.Types;

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
        descriptor.Field(p => p.Id).Type<NonNullType<UuidType>>();
    }
}

[ObjectType<PurchaseOrder>]
internal static partial class PurchaseOrderNode
{
    public static async Task<Vendor?> GetVendorAsync(
        [Parent(requires: nameof(PurchaseOrder.VendorId))] PurchaseOrder po,
        QueryContext<Vendor> query,
        VendorByIdDataLoader vendorById,
        CancellationToken cancellationToken)
        => await vendorById.With(query).LoadAsync(po.VendorId, cancellationToken);

    public static async Task<List<PurchaseOrderLine>> GetLinesAsync(
        [Parent(requires: nameof(PurchaseOrder.Id))] PurchaseOrder po,
        QueryContext<PurchaseOrderLine> query,
        PoLinesByPoIdDataLoader poLinesByPoId,
        CancellationToken cancellationToken)
        => await poLinesByPoId.With(query).LoadAsync(po.Id, cancellationToken) ?? [];

    static partial void Configure(IObjectTypeDescriptor<PurchaseOrder> descriptor)
    {
        descriptor.Ignore(p => p.Vendor);
        descriptor.Ignore(p => p.Lines);
        descriptor.Ignore(p => p.GoodsReceipts);
    }
}

[ObjectType<Vendor>]
internal static partial class VendorNode
{
    static partial void Configure(IObjectTypeDescriptor<Vendor> descriptor)
    {
        descriptor.Ignore(v => v.PurchaseOrders);
    }
}

[ObjectType<PurchaseOrderLine>]
internal static partial class PurchaseOrderLineNode
{
    public static ProductEntityStub GetProduct(
        [Parent(requires: nameof(PurchaseOrderLine.ProductId))] PurchaseOrderLine line)
        => new ProductEntityStub { Id = line.ProductId };

    static partial void Configure(IObjectTypeDescriptor<PurchaseOrderLine> descriptor)
    {
        descriptor.Ignore(l => l.PurchaseOrder);
    }
}

[ObjectType<GoodsReceipt>]
internal static partial class GoodsReceiptNode
{
    public static async Task<PurchaseOrder?> GetPurchaseOrderAsync(
        [Parent(requires: nameof(GoodsReceipt.PurchaseOrderId))] GoodsReceipt receipt,
        QueryContext<PurchaseOrder> query,
        PurchaseOrderByIdDataLoader purchaseOrderById,
        CancellationToken cancellationToken)
        => await purchaseOrderById.With(query).LoadAsync(receipt.PurchaseOrderId, cancellationToken);

    static partial void Configure(IObjectTypeDescriptor<GoodsReceipt> descriptor)
    {
        descriptor.Ignore(g => g.PurchaseOrder);
    }
}
