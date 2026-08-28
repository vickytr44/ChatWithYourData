using ChatWithYourData.ProcurementService.API.GraphQL.Errors;
using ChatWithYourData.ProcurementService.Application.Features.Procurement.Commands;
using ChatWithYourData.ProcurementService.Application.Features.Procurement.DTOs;
using ChatWithYourData.ProcurementService.Domain.Entities;
using HotChocolate.Types;
using MediatR;

namespace ChatWithYourData.ProcurementService.API.GraphQL.Mutations;

[MutationType]
internal static partial class ProcurementMutations
{
    [Error(typeof(VendorCodeAlreadyExistsException))]
    public static async Task<Vendor> CreateVendorAsync(
        string name,
        string contactEmail,
        string phone,
        string address,
        int paymentTermsDays,
        string taxId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateVendorCommand(name, contactEmail, phone, address, paymentTermsDays, taxId);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            throw new VendorCodeAlreadyExistsException(result.Error!);

        return new Vendor
        {
            Id = result.Value!.Id,
            VendorCode = result.Value.VendorCode,
            Name = result.Value.Name,
            ContactEmail = result.Value.ContactEmail,
            Phone = result.Value.Phone,
            Address = result.Value.Address,
            PaymentTermsDays = result.Value.PaymentTermsDays,
            TaxId = result.Value.TaxId,
            IsActive = result.Value.IsActive
        };
    }

    [Error(typeof(VendorNotFoundException))]
    public static async Task<PurchaseOrder> CreatePurchaseOrderAsync(
        Guid vendorId,
        DateTime? expectedDeliveryDateUtc,
        string notes,
        List<CreatePoLineInput> lines,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreatePurchaseOrderCommand(vendorId, expectedDeliveryDateUtc, notes, lines);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            throw new VendorNotFoundException(vendorId);

        return new PurchaseOrder
        {
            Id = result.Value!.Id,
            PoNumber = result.Value.PoNumber,
            VendorId = result.Value.VendorId,
            OrderDateUtc = result.Value.OrderDateUtc,
            ExpectedDeliveryDateUtc = result.Value.ExpectedDeliveryDateUtc,
            Status = result.Value.Status,
            TotalCost = result.Value.TotalCost,
            Notes = result.Value.Notes
        };
    }
}
