using ChatWithYourData.SalesService.Application.Common.Interfaces;
using ChatWithYourData.SalesService.Application.Features.Sales.Commands;
using ChatWithYourData.SalesService.Application.Features.Sales.DTOs;
using ChatWithYourData.SalesService.Domain.Entities;
using ChatWithYourData.SalesService.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ChatWithYourData.SalesService.UnitTests;

public class CreateCustomerCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MockSalesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new MockSalesDbContext(options);
        dbContext.Customers.Add(new Customer
        {
            CustomerNumber = "CUST-00001",
            Name = "Existing Customer",
            Email = "duplicate@company.com"
        });
        await dbContext.SaveChangesAsync();

        var handler = new CreateCustomerCommandHandler(dbContext);
        var command = new CreateCustomerCommand("Duplicate Customer", "duplicate@company.com", "+1234567890", "123 Main St", "123 Main St", 10000);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ReturnsSuccessWithCustomerDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MockSalesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new MockSalesDbContext(options);
        var handler = new CreateCustomerCommandHandler(dbContext);
        var command = new CreateCustomerCommand("New Tech Inc", "contact@newtech.io", "+1987654321", "456 Market St", "456 Market St", 25000);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("New Tech Inc");
        result.Value.Email.Should().Be("contact@newtech.io");
        result.Value.CustomerNumber.Should().StartWith("CUST-");
    }
}

public class CreateSalesOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCustomerNotFound_ReturnsFailureResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MockSalesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new MockSalesDbContext(options);
        var handler = new CreateSalesOrderCommandHandler(dbContext);
        var command = new CreateSalesOrderCommand(Guid.NewGuid(), "Order notes", new List<CreateSalesOrderLineInput>
        {
            new(Guid.NewGuid(), "SKU-001", "Product 1", 2, 100m, 0)
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_WhenValidCommand_CalculatesTotalAndCreatesOrder()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MockSalesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new MockSalesDbContext(options);
        var customerId = Guid.NewGuid();
        dbContext.Customers.Add(new Customer { Id = customerId, CustomerNumber = "CUST-001", Name = "Valid Customer", Email = "valid@customer.com" });
        await dbContext.SaveChangesAsync();

        var handler = new CreateSalesOrderCommandHandler(dbContext);
        var command = new CreateSalesOrderCommand(customerId, "Deliver to HQ", new List<CreateSalesOrderLineInput>
        {
            new(Guid.NewGuid(), "SKU-A", "Item A", 2, 500m, 10m), // 2 * 500 * 0.9 = 900
            new(Guid.NewGuid(), "SKU-B", "Item B", 1, 100m, 0m)   // 100
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalAmount.Should().Be(1000m);
        result.Value.Lines.Should().HaveCount(2);
        result.Value.Status.Should().Be(OrderStatus.Confirmed);
    }
}

public class MockSalesDbContext(DbContextOptions<MockSalesDbContext> options) : DbContext(options), ISalesDbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
}
