namespace ChatWithYourData.InventoryService.API.GraphQL.Errors;

public sealed class DuplicateSkuException(string sku)
    : Exception($"Product with SKU '{sku}' already exists.")
{
    public string Sku { get; } = sku;
}

public sealed class ProductNotFoundException(Guid productId)
    : Exception($"Product with ID {productId} was not found.")
{
    public Guid ProductId { get; } = productId;
}

public sealed class WarehouseNotFoundException(Guid warehouseId)
    : Exception($"Warehouse with ID {warehouseId} was not found.")
{
    public Guid WarehouseId { get; } = warehouseId;
}

public sealed class InsufficientStockException(int currentStock, int requestedDelta)
    : Exception($"Insufficient stock. Current: {currentStock}, Requested delta: {requestedDelta}")
{
    public int CurrentStock { get; } = currentStock;
    public int RequestedDelta { get; } = requestedDelta;
}

public sealed class UninitializedStockException(Guid productId, Guid warehouseId)
    : Exception($"Cannot reduce stock for uninitialized stock item (Product: {productId}, Warehouse: {warehouseId}).")
{
    public Guid ProductId { get; } = productId;
    public Guid WarehouseId { get; } = warehouseId;
}
