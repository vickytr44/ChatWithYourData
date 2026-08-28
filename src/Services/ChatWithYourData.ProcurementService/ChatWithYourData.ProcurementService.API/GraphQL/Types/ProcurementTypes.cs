using ChatWithYourData.ProcurementService.API.GraphQL.DataLoaders;
using ChatWithYourData.ProcurementService.Domain.Entities;
using HotChocolate.Types;

namespace ChatWithYourData.ProcurementService.API.GraphQL.Types;

public class PurchaseOrderType : ObjectType<PurchaseOrder>
{
    protected override void Configure(IObjectTypeDescriptor<PurchaseOrder> descriptor)
    {
        descriptor.Description("Represents a purchase order issued to a vendor.");

        descriptor.Field(p => p.VendorId)
            .IsProjected(true);

        descriptor.Field(p => p.Vendor)
            .ResolveWith<PurchaseOrderResolvers>(r => r.GetVendorAsync(default!, default!, default!))
            .Description("The vendor for this purchase order (resolved via DataLoader).");

        descriptor.Field(p => p.Lines)
            .ResolveWith<PurchaseOrderResolvers>(r => r.GetLinesAsync(default!, default!, default!))
            .Description("The line items for this purchase order (resolved via DataLoader).");
    }

    private class PurchaseOrderResolvers
    {
        public async Task<Vendor?> GetVendorAsync(
            [Parent] PurchaseOrder po,
            VendorByIdDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            return await dataLoader.LoadAsync(po.VendorId, cancellationToken);
        }

        public async Task<List<PurchaseOrderLine>> GetLinesAsync(
            [Parent] PurchaseOrder po,
            PoLinesByPoIdDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            var lines = await dataLoader.LoadAsync(po.Id, cancellationToken);
            return lines ?? new List<PurchaseOrderLine>();
        }
    }
}
