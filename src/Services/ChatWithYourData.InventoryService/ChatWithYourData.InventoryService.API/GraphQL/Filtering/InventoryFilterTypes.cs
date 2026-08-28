using ChatWithYourData.InventoryService.Domain.Entities;
using HotChocolate.Data.Filters;

namespace ChatWithYourData.InventoryService.API.GraphQL.Filtering;

public sealed class ProductFilterInputType : FilterInputType<Product>
{
    protected override void Configure(IFilterInputTypeDescriptor<Product> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.Name);
        descriptor.Field(t => t.Sku);
        descriptor.Field(t => t.IsActive);
        descriptor.Field(t => t.CategoryId);
    }
}

public sealed class CategoryFilterInputType : FilterInputType<Category>
{
    protected override void Configure(IFilterInputTypeDescriptor<Category> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.Name);
        descriptor.Field(t => t.ParentCategoryId);
    }
}

public sealed class WarehouseFilterInputType : FilterInputType<Warehouse>
{
    protected override void Configure(IFilterInputTypeDescriptor<Warehouse> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.Name);
        descriptor.Field(t => t.Code);
        descriptor.Field(t => t.IsActive);
    }
}

public sealed class StockItemFilterInputType : FilterInputType<StockItem>
{
    protected override void Configure(IFilterInputTypeDescriptor<StockItem> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.ProductId);
        descriptor.Field(t => t.WarehouseId);
        descriptor.Field(t => t.QuantityOnHand);
        descriptor.Field(t => t.ReorderPoint);
    }
}
