using ChatWithYourData.InventoryService.API.GraphQL.Errors;
using ChatWithYourData.InventoryService.Application.Features.Products.Commands;
using ChatWithYourData.InventoryService.Domain.Entities;
using ChatWithYourData.InventoryService.Domain.Enums;
using HotChocolate.Types;
using MediatR;

namespace ChatWithYourData.InventoryService.API.GraphQL.Mutations;

[MutationType]
internal static partial class InventoryMutations
{
    [Error(typeof(DuplicateSkuException))]
    public static async Task<Product> CreateProductAsync(
        string sku,
        string name,
        string description,
        decimal unitPrice,
        string unitOfMeasure,
        Guid categoryId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(sku, name, description, unitPrice, unitOfMeasure, categoryId);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            throw new DuplicateSkuException(sku);

        return new Product
        {
            Id = result.Value!.Id,
            Sku = result.Value.Sku,
            Name = result.Value.Name,
            Description = result.Value.Description,
            UnitPrice = result.Value.UnitPrice,
            UnitOfMeasure = result.Value.UnitOfMeasure,
            IsActive = result.Value.IsActive,
            CategoryId = result.Value.CategoryId,
            CreatedAtUtc = result.Value.CreatedAtUtc
        };
    }

    [Error(typeof(ProductNotFoundException))]
    [Error(typeof(WarehouseNotFoundException))]
    [Error(typeof(InsufficientStockException))]
    [Error(typeof(UninitializedStockException))]
    public static async Task<StockItem> AdjustStockAsync(
        Guid productId,
        Guid warehouseId,
        int quantityDelta,
        AdjustmentReason reason,
        string notes,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new AdjustStockCommand(productId, warehouseId, quantityDelta, reason, notes);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            var error = result.Error!;
            if (error.Contains("Product"))
                throw new ProductNotFoundException(productId);
            if (error.Contains("Warehouse"))
                throw new WarehouseNotFoundException(warehouseId);
            if (error.Contains("Insufficient"))
                throw new InsufficientStockException(0, quantityDelta);
            if (error.Contains("uninitialized"))
                throw new UninitializedStockException(productId, warehouseId);
            throw new InvalidOperationException(error);
        }

        return new StockItem
        {
            Id = result.Value!.Id,
            ProductId = result.Value.ProductId,
            WarehouseId = result.Value.WarehouseId,
            QuantityOnHand = result.Value.QuantityOnHand,
            QuantityReserved = result.Value.QuantityReserved,
            ReorderPoint = result.Value.ReorderPoint
        };
    }
}
