using ChatWithYourData.InventoryService.API.GraphQL.DataLoaders;
using ChatWithYourData.InventoryService.Domain.Entities;
using HotChocolate.Types;

namespace ChatWithYourData.InventoryService.API.GraphQL.Types;

public class ProductType : ObjectType<Product>
{
    protected override void Configure(IObjectTypeDescriptor<Product> descriptor)
    {
        descriptor.Description("Represents a product within the inventory catalog.");

        descriptor.Field(p => p.Category)
            .ResolveWith<ProductResolvers>(r => r.GetCategoryAsync(default!, default!, default!))
            .Description("The category the product belongs to (resolved via DataLoader).");
    }

    private class ProductResolvers
    {
        public async Task<Category?> GetCategoryAsync(
            [Parent] Product product,
            CategoryByIdDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            return await dataLoader.LoadAsync(product.CategoryId, cancellationToken);
        }
    }
}

public class StockItemType : ObjectType<StockItem>
{
    protected override void Configure(IObjectTypeDescriptor<StockItem> descriptor)
    {
        descriptor.Description("Represents stock level for a product in a warehouse.");

        descriptor.Field(s => s.Product)
            .ResolveWith<StockItemResolvers>(r => r.GetProductAsync(default!, default!, default!))
            .Description("The product associated with this stock item (resolved via DataLoader).");

        descriptor.Field(s => s.Warehouse)
            .ResolveWith<StockItemResolvers>(r => r.GetWarehouseAsync(default!, default!, default!))
            .Description("The warehouse storing this stock item (resolved via DataLoader).");
    }

    private class StockItemResolvers
    {
        public async Task<Product?> GetProductAsync(
            [Parent] StockItem stockItem,
            ProductByIdDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            return await dataLoader.LoadAsync(stockItem.ProductId, cancellationToken);
        }

        public async Task<Warehouse?> GetWarehouseAsync(
            [Parent] StockItem stockItem,
            WarehouseByIdDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            return await dataLoader.LoadAsync(stockItem.WarehouseId, cancellationToken);
        }
    }
}
