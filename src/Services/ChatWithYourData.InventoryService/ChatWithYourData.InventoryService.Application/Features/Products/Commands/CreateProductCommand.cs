using ChatWithYourData.InventoryService.Application.Common;
using ChatWithYourData.InventoryService.Application.Common.Interfaces;
using ChatWithYourData.InventoryService.Application.Features.Products.DTOs;
using ChatWithYourData.InventoryService.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.InventoryService.Application.Features.Products.Commands;

public record CreateProductCommand(
    string Sku,
    string Name,
    string Description,
    decimal UnitPrice,
    string UnitOfMeasure,
    Guid CategoryId
) : IRequest<Result<ProductDto>>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}

public class CreateProductCommandHandler(IInventoryDbContext dbContext) 
    : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var skuExists = await dbContext.Products.AnyAsync(p => p.Sku == request.Sku, cancellationToken);
        if (skuExists)
        {
            return Result<ProductDto>.Failure($"Product with SKU '{request.Sku}' already exists.");
        }

        var product = new Product
        {
            Sku = request.Sku,
            Name = request.Name,
            Description = request.Description,
            UnitPrice = request.UnitPrice,
            UnitOfMeasure = string.IsNullOrWhiteSpace(request.UnitOfMeasure) ? "Units" : request.UnitOfMeasure,
            CategoryId = request.CategoryId,
            IsActive = true
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = new ProductDto(
            product.Id,
            product.Sku,
            product.Name,
            product.Description,
            product.UnitPrice,
            product.UnitOfMeasure,
            product.IsActive,
            product.CategoryId,
            product.CreatedAtUtc
        );

        return Result<ProductDto>.Success(dto);
    }
}
