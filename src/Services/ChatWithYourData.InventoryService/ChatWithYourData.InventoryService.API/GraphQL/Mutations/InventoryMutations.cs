using ChatWithYourData.InventoryService.Application.Features.Products.Commands;
using ChatWithYourData.InventoryService.Application.Features.Products.DTOs;
using ChatWithYourData.InventoryService.Domain.Enums;
using HotChocolate;
using HotChocolate.Types;
using MediatR;

namespace ChatWithYourData.InventoryService.API.GraphQL.Mutations;

public record CreateProductInput(
    string Sku,
    string Name,
    string Description,
    decimal UnitPrice,
    string UnitOfMeasure,
    Guid CategoryId
);

public record AdjustStockInput(
    Guid ProductId,
    Guid WarehouseId,
    int QuantityDelta,
    AdjustmentReason Reason,
    string Notes
);

public record MutationPayload<T>(bool Success, T? Data, string? Error);

public class InventoryMutations
{
    public async Task<MutationPayload<ProductDto>> CreateProductAsync(
        CreateProductInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            input.Sku,
            input.Name,
            input.Description,
            input.UnitPrice,
            input.UnitOfMeasure,
            input.CategoryId
        );

        var result = await mediator.Send(command, cancellationToken);
        return new MutationPayload<ProductDto>(result.IsSuccess, result.Value, result.Error);
    }

    public async Task<MutationPayload<StockItemDto>> AdjustStockAsync(
        AdjustStockInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new AdjustStockCommand(
            input.ProductId,
            input.WarehouseId,
            input.QuantityDelta,
            input.Reason,
            input.Notes
        );

        var result = await mediator.Send(command, cancellationToken);
        return new MutationPayload<StockItemDto>(result.IsSuccess, result.Value, result.Error);
    }
}
