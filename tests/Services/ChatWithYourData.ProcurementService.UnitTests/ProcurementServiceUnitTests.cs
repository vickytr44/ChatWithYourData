using ChatWithYourData.ProcurementService.Application.Common.Interfaces;
using ChatWithYourData.ProcurementService.Application.Features.Procurement.Commands;
using ChatWithYourData.ProcurementService.Application.Features.Procurement.DTOs;
using ChatWithYourData.ProcurementService.Domain.Entities;
using ChatWithYourData.ProcurementService.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ChatWithYourData.ProcurementService.UnitTests;

public class CreateVendorCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MockProcurementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new MockProcurementDbContext(options);
        dbContext.Vendors.Add(new Vendor
        {
            VendorCode = "VND-00001",
            Name = "Existing Vendor",
            ContactEmail = "supplier@chips.com"
        });
        await dbContext.SaveChangesAsync();

        var handler = new CreateVendorCommandHandler(dbContext);
        var command = new CreateVendorCommand("Duplicate Vendor", "supplier@chips.com", "+1234567890", "123 Tech Blvd", 30, "TAX-123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ReturnsSuccessWithVendorDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MockProcurementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new MockProcurementDbContext(options);
        var handler = new CreateVendorCommandHandler(dbContext);
        var command = new CreateVendorCommand("Quantum Photonics", "info@quantumphotonics.com", "+1555444333", "77 Laser Way", 60, "TAX-9988");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Quantum Photonics");
        result.Value.PaymentTermsDays.Should().Be(60);
        result.Value.VendorCode.Should().StartWith("VND-");
    }
}

public class CreatePurchaseOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenVendorNotFound_ReturnsFailureResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MockProcurementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new MockProcurementDbContext(options);
        var handler = new CreatePurchaseOrderCommandHandler(dbContext);
        var command = new CreatePurchaseOrderCommand(Guid.NewGuid(), null, "PO Notes", new List<CreatePoLineInput>
        {
            new(Guid.NewGuid(), "SKU-X", "Part X", 10, 50m)
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_WhenValidCommand_CalculatesTotalAndCreatesPo()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MockProcurementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new MockProcurementDbContext(options);
        var vendorId = Guid.NewGuid();
        dbContext.Vendors.Add(new Vendor { Id = vendorId, VendorCode = "VND-001", Name = "Supplier A", ContactEmail = "supp@a.com" });
        await dbContext.SaveChangesAsync();

        var handler = new CreatePurchaseOrderCommandHandler(dbContext);
        var command = new CreatePurchaseOrderCommand(vendorId, DateTime.UtcNow.AddDays(7), "Urgent parts", new List<CreatePoLineInput>
        {
            new(Guid.NewGuid(), "SKU-1", "Component 1", 100, 25m), // 2500
            new(Guid.NewGuid(), "SKU-2", "Component 2", 20, 100m)   // 2000
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCost.Should().Be(4500m);
        result.Value.Lines.Should().HaveCount(2);
        result.Value.Status.Should().Be(PurchaseOrderStatus.Submitted);
    }
}

public class MockProcurementDbContext(DbContextOptions<MockProcurementDbContext> options) : DbContext(options), IProcurementDbContext
{
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
}
