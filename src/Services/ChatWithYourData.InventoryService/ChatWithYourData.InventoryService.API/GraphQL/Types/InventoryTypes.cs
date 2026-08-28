using ChatWithYourData.InventoryService.API.GraphQL.DataLoaders;
using ChatWithYourData.InventoryService.Domain.Entities;
using GreenDonut.Data;
using HotChocolate.Types;

namespace ChatWithYourData.InventoryService.API.GraphQL.Types;

[ObjectType<Product>]
internal static partial class ProductNode
{
    public static async Task<Category?> GetCategoryAsync(
        [Parent(requires: nameof(Product.CategoryId))] Product product,
        QueryContext<Category> query,
        CategoryByIdDataLoader categoryById,
        CancellationToken cancellationToken)
        => await categoryById.With(query).LoadAsync(product.CategoryId, cancellationToken);

    public static async Task<List<StockItem>> GetStockItemsAsync(
        [Parent(requires: nameof(Product.Id))] Product product,
        QueryContext<StockItem> query,
        StockItemsByProductIdDataLoader stockItemsByProductId,
        CancellationToken cancellationToken)
        => await stockItemsByProductId.With(query).LoadAsync(product.Id, cancellationToken) ?? [];

    static partial void Configure(IObjectTypeDescriptor<Product> descriptor)
    {
        descriptor.Ignore(p => p.Category);
        descriptor.Ignore(p => p.StockItems);
        descriptor.Ignore(p => p.StockAdjustments);
    }
}

[ObjectType<StockItem>]
internal static partial class StockItemNode
{
    public static async Task<Product?> GetProductAsync(
        [Parent(requires: nameof(StockItem.ProductId))] StockItem stockItem,
        QueryContext<Product> query,
        ProductByIdDataLoader productById,
        CancellationToken cancellationToken)
        => await productById.With(query).LoadAsync(stockItem.ProductId, cancellationToken);

    public static async Task<Warehouse?> GetWarehouseAsync(
        [Parent(requires: nameof(StockItem.WarehouseId))] StockItem stockItem,
        QueryContext<Warehouse> query,
        WarehouseByIdDataLoader warehouseById,
        CancellationToken cancellationToken)
        => await warehouseById.With(query).LoadAsync(stockItem.WarehouseId, cancellationToken);

    static partial void Configure(IObjectTypeDescriptor<StockItem> descriptor)
    {
        descriptor.Ignore(s => s.Product);
        descriptor.Ignore(s => s.Warehouse);
    }
}

[ObjectType<Category>]
internal static partial class CategoryNode
{
    static partial void Configure(IObjectTypeDescriptor<Category> descriptor)
    {
        descriptor.Ignore(c => c.ParentCategory);
        descriptor.Ignore(c => c.SubCategories);
        descriptor.Ignore(c => c.Products);
    }
}

[ObjectType<Warehouse>]
internal static partial class WarehouseNode
{
    static partial void Configure(IObjectTypeDescriptor<Warehouse> descriptor)
    {
        descriptor.Ignore(w => w.StockItems);
    }
}
