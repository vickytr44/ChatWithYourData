using ChatWithYourData.InventoryService.Application.Common;
using ChatWithYourData.InventoryService.Application.Common.Interfaces;
using ChatWithYourData.InventoryService.Application.Features.Products.DTOs;
using ChatWithYourData.InventoryService.Domain.Entities;
using ChatWithYourData.InventoryService.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.InventoryService.Application.Features.Products.Commands;

public record AdjustStockCommand(
    Guid ProductId,
    Guid WarehouseId,
    int QuantityDelta,
    AdjustmentReason Reason,
    string Notes
) : IRequest<Result<StockItemDto>>;

public class AdjustStockCommandValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.QuantityDelta).NotEqual(0);
    }
}

public class AdjustStockCommandHandler(IInventoryDbContext dbContext)
    : IRequestHandler<AdjustStockCommand, Result<StockItemDto>>
{
    public async Task<Result<StockItemDto>> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.FindAsync([request.ProductId], cancellationToken);
        if (product == null)
            return Result<StockItemDto>.Failure($"Product with ID {request.ProductId} was not found.");

        var warehouse = await dbContext.Warehouses.FindAsync([request.WarehouseId], cancellationToken);
        if (warehouse == null)
            return Result<StockItemDto>.Failure($"Warehouse with ID {request.WarehouseId} was not found.");

        var stockItem = await dbContext.StockItems
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId && s.WarehouseId == request.WarehouseId, cancellationToken);

        if (stockItem == null)
        {
            if (request.QuantityDelta < 0)
                return Result<StockItemDto>.Failure("Cannot reduce stock for an uninitialized stock item.");

            stockItem = new StockItem
            {
                ProductId = request.ProductId,
                WarehouseId = request.WarehouseId,
                QuantityOnHand = request.QuantityDelta,
                QuantityReserved = 0,
                ReorderPoint = 10
            };
            dbContext.StockItems.Add(stockItem);
        }
        else
        {
            if (stockItem.QuantityOnHand + request.QuantityDelta < 0)
                return Result<StockItemDto>.Failure($"Insufficient stock. Current: {stockItem.QuantityOnHand}, Requested delta: {request.QuantityDelta}");

            stockItem.QuantityOnHand += request.QuantityDelta;
            stockItem.UpdatedAtUtc = DateTime.UtcNow;
        }

        var adjustment = new StockAdjustment
        {
            ProductId = request.ProductId,
            WarehouseId = request.WarehouseId,
            QuantityDelta = request.QuantityDelta,
            Reason = request.Reason,
            Notes = request.Notes,
            AdjustedAtUtc = DateTime.UtcNow
        };
        dbContext.StockAdjustments.Add(adjustment);

        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = new StockItemDto(
            stockItem.Id,
            stockItem.ProductId,
            stockItem.WarehouseId,
            stockItem.QuantityOnHand,
            stockItem.QuantityReserved,
            stockItem.ReorderPoint
        );

        return Result<StockItemDto>.Success(dto);
    }
}
