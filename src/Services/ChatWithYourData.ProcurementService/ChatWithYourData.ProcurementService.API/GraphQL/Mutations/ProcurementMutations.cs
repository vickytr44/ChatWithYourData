using ChatWithYourData.ProcurementService.Application.Features.Procurement.Commands;
using ChatWithYourData.ProcurementService.Application.Features.Procurement.DTOs;
using HotChocolate;
using HotChocolate.Types;
using MediatR;

namespace ChatWithYourData.ProcurementService.API.GraphQL.Mutations;

public record CreateVendorInput(
    string Name,
    string ContactEmail,
    string Phone,
    string Address,
    int PaymentTermsDays,
    string TaxId
);

public record CreatePurchaseOrderInput(
    Guid VendorId,
    DateTime? ExpectedDeliveryDateUtc,
    string Notes,
    List<CreatePoLineInput> Lines
);

public record ProcurementMutationPayload<T>(bool Success, T? Data, string? Error);

public class ProcurementMutations
{
    public async Task<ProcurementMutationPayload<VendorDto>> CreateVendorAsync(
        CreateVendorInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateVendorCommand(
            input.Name,
            input.ContactEmail,
            input.Phone,
            input.Address,
            input.PaymentTermsDays,
            input.TaxId
        );

        var result = await mediator.Send(command, cancellationToken);
        return new ProcurementMutationPayload<VendorDto>(result.IsSuccess, result.Value, result.Error);
    }

    public async Task<ProcurementMutationPayload<PurchaseOrderDto>> CreatePurchaseOrderAsync(
        CreatePurchaseOrderInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreatePurchaseOrderCommand(
            input.VendorId,
            input.ExpectedDeliveryDateUtc,
            input.Notes,
            input.Lines
        );

        var result = await mediator.Send(command, cancellationToken);
        return new ProcurementMutationPayload<PurchaseOrderDto>(result.IsSuccess, result.Value, result.Error);
    }
}
