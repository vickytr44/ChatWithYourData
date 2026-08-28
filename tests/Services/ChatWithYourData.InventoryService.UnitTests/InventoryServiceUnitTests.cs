using ChatWithYourData.InventoryService.Application.Common.Interfaces;
using ChatWithYourData.InventoryService.Application.Features.Products.Commands;
using ChatWithYourData.InventoryService.Domain.Entities;
using ChatWithYourData.InventoryService.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ChatWithYourData.InventoryService.UnitTests;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IInventoryDbContext> _dbContextMock;
    private readonly List<Product> _products = new();

    public CreateProductCommandHandlerTests()
    {
        _dbContextMock = new Mock<IInventoryDbContext>();
    }

    [Fact]
    public async Task Handle_WhenSkuAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MockInventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new MockInventoryDbContext(options);
        dbContext.Products.Add(new Product
        {
            Sku = "PRD-EXISTING",
            Name = "Existing Product",
            CategoryId = Guid.NewGuid()
        });
        await dbContext.SaveChangesAsync();

        var handler = new CreateProductCommandHandler(dbContext);
        var command = new CreateProductCommand("PRD-EXISTING", "Duplicate Product", "Desc", 99.99m, "Units", Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ReturnsSuccessWithProductDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MockInventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new MockInventoryDbContext(options);
        var handler = new CreateProductCommandHandler(dbContext);
        var categoryId = Guid.NewGuid();
        var command = new CreateProductCommand("PRD-NEW-001", "Gaming Mouse", "Wireless mouse", 79.99m, "Units", categoryId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Sku.Should().Be("PRD-NEW-001");
        result.Value.Name.Should().Be("Gaming Mouse");
        result.Value.UnitPrice.Should().Be(79.99m);
        result.Value.CategoryId.Should().Be(categoryId);

        var savedProduct = await dbContext.Products.FirstOrDefaultAsync(p => p.Sku == "PRD-NEW-001");
        savedProduct.Should().NotBeNull();
        savedProduct!.Name.Should().Be("Gaming Mouse");
    }
}

public class AdjustStockCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenProductNotFound_ReturnsFailureResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MockInventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new MockInventoryDbContext(options);
        var handler = new AdjustStockCommandHandler(dbContext);
        var command = new AdjustStockCommand(Guid.NewGuid(), Guid.NewGuid(), 10, AdjustmentReason.Restock, "Initial stock");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("was not found");
    }

    [Fact]
    public async Task Handle_WhenStockItemExists_UpdatesQuantityOnHandCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MockInventoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new MockInventoryDbContext(options);
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();

        dbContext.Products.Add(new Product { Id = productId, Sku = "SKU-TEST", Name = "Test Product", CategoryId = Guid.NewGuid() });
        dbContext.Warehouses.Add(new Warehouse { Id = warehouseId, Code = "WH-TEST", Name = "Test Warehouse" });
        dbContext.StockItems.Add(new StockItem { ProductId = productId, WarehouseId = warehouseId, QuantityOnHand = 50, QuantityReserved = 5, ReorderPoint = 10 });
        await dbContext.SaveChangesAsync();

        var handler = new AdjustStockCommandHandler(dbContext);
        var command = new AdjustStockCommand(productId, warehouseId, -15, AdjustmentReason.Damage, "Damaged in transit");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.QuantityOnHand.Should().Be(35);

        var stockItem = await dbContext.StockItems.FirstAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId);
        stockItem.QuantityOnHand.Should().Be(35);

        var adjustment = await dbContext.StockAdjustments.FirstOrDefaultAsync(a => a.ProductId == productId);
        adjustment.Should().NotBeNull();
        adjustment!.QuantityDelta.Should().Be(-15);
    }
}

public class MockInventoryDbContext(DbContextOptions<MockInventoryDbContext> options) : DbContext(options), IInventoryDbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();
}
