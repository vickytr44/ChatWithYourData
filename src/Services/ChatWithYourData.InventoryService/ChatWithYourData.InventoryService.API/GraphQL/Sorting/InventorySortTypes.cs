using ChatWithYourData.InventoryService.Domain.Entities;
using HotChocolate.Data.Sorting;

namespace ChatWithYourData.InventoryService.API.GraphQL.Sorting;

public sealed class ProductSortInputType : SortInputType<Product>
{
    protected override void Configure(ISortInputTypeDescriptor<Product> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.Name);
        descriptor.Field(t => t.Sku);
        descriptor.Field(t => t.UnitPrice);
        descriptor.Field(t => t.CreatedAtUtc);
    }
}

public sealed class CategorySortInputType : SortInputType<Category>
{
    protected override void Configure(ISortInputTypeDescriptor<Category> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.Name);
    }
}

public sealed class WarehouseSortInputType : SortInputType<Warehouse>
{
    protected override void Configure(ISortInputTypeDescriptor<Warehouse> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.Name);
        descriptor.Field(t => t.Code);
    }
}

public sealed class StockItemSortInputType : SortInputType<StockItem>
{
    protected override void Configure(ISortInputTypeDescriptor<StockItem> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.QuantityOnHand);
        descriptor.Field(t => t.ReorderPoint);
    }
}
