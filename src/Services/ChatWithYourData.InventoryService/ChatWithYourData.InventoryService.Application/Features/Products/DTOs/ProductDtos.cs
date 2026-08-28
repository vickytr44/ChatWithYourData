namespace ChatWithYourData.InventoryService.Application.Features.Products.DTOs;

public record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    string Description,
    decimal UnitPrice,
    string UnitOfMeasure,
    bool IsActive,
    Guid CategoryId,
    DateTime CreatedAtUtc
);

public record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentCategoryId
);

public record StockItemDto(
    Guid Id,
    Guid ProductId,
    Guid WarehouseId,
    int QuantityOnHand,
    int QuantityReserved,
    int ReorderPoint
);
