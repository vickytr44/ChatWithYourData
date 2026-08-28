using ChatWithYourData.InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.InventoryService.Infrastructure.Persistence;

public static class InventoryDbSeeder
{
    public static async Task SeedAsync(InventoryDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.Categories.AnyAsync())
            return;

        var electronics = new Category { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Electronics", Description = "Electronic hardware and gadgets" };
        var computers = new Category { Id = Guid.Parse("11111111-1111-1111-1111-222222222222"), Name = "Computers", Description = "Laptops, Desktops, and Servers", ParentCategoryId = electronics.Id };
        var accessories = new Category { Id = Guid.Parse("11111111-1111-1111-1111-333333333333"), Name = "Accessories", Description = "Cables, adapters, peripherals", ParentCategoryId = electronics.Id };

        dbContext.Categories.AddRange(electronics, computers, accessories);

        var laptop = new Product
        {
            Id = Guid.Parse("22222222-2222-2222-2222-111111111111"),
            Sku = "PRD-LAP-001",
            Name = "Enterprise Pro Laptop 16\"",
            Description = "High-performance workstation laptop with 32GB RAM",
            UnitPrice = 1899.99m,
            UnitOfMeasure = "Units",
            CategoryId = computers.Id,
            IsActive = true
        };

        var monitor = new Product
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Sku = "PRD-MON-002",
            Name = "UltraWide 34\" 4K Monitor",
            Description = "Color-accurate curved display with USB-C Hub",
            UnitPrice = 649.50m,
            UnitOfMeasure = "Units",
            CategoryId = accessories.Id,
            IsActive = true
        };

        var keyboard = new Product
        {
            Id = Guid.Parse("22222222-2222-2222-2222-333333333333"),
            Sku = "PRD-KBD-003",
            Name = "Mechanical Wireless Keyboard",
            Description = "Tactile quiet switches with multi-device Bluetooth",
            UnitPrice = 129.00m,
            UnitOfMeasure = "Units",
            CategoryId = accessories.Id,
            IsActive = true
        };

        dbContext.Products.AddRange(laptop, monitor, keyboard);

        var mainWarehouse = new Warehouse
        {
            Id = Guid.Parse("33333333-3333-3333-3333-111111111111"),
            Code = "WH-CENTRAL",
            Name = "Central Distribution Hub",
            LocationAddress = "100 Logistics Blvd, Dallas, TX 75201",
            IsActive = true
        };

        var westWarehouse = new Warehouse
        {
            Id = Guid.Parse("33333333-3333-3333-3333-222222222222"),
            Code = "WH-WEST",
            Name = "West Coast Facility",
            LocationAddress = "500 Harbor Way, Seattle, WA 98101",
            IsActive = true
        };

        dbContext.Warehouses.AddRange(mainWarehouse, westWarehouse);

        var stock1 = new StockItem { ProductId = laptop.Id, WarehouseId = mainWarehouse.Id, QuantityOnHand = 150, QuantityReserved = 12, ReorderPoint = 20 };
        var stock2 = new StockItem { ProductId = laptop.Id, WarehouseId = westWarehouse.Id, QuantityOnHand = 45, QuantityReserved = 5, ReorderPoint = 15 };
        var stock3 = new StockItem { ProductId = monitor.Id, WarehouseId = mainWarehouse.Id, QuantityOnHand = 80, QuantityReserved = 8, ReorderPoint = 10 };
        var stock4 = new StockItem { ProductId = keyboard.Id, WarehouseId = mainWarehouse.Id, QuantityOnHand = 320, QuantityReserved = 25, ReorderPoint = 50 };

        dbContext.StockItems.AddRange(stock1, stock2, stock3, stock4);

        await dbContext.SaveChangesAsync();
    }
}
