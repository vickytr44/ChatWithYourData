using ChatWithYourData.SalesService.Domain.Entities;
using ChatWithYourData.SalesService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.SalesService.Infrastructure.Persistence;

public static class SalesDbSeeder
{
    public static async Task SeedAsync(SalesDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.Customers.AnyAsync())
            return;

        var customer1 = new Customer
        {
            Id = Guid.Parse("44444444-4444-4444-4444-111111111111"),
            CustomerNumber = "CUST-00001",
            Name = "Acme Technologies Corp",
            Email = "procurement@acmetech.io",
            Phone = "+1 (555) 234-5678",
            BillingAddress = "100 Innovation Way, Austin, TX 78701",
            ShippingAddress = "100 Innovation Way, Austin, TX 78701",
            CreditLimit = 50000.00m,
            IsActive = true
        };

        var customer2 = new Customer
        {
            Id = Guid.Parse("44444444-4444-4444-4444-222222222222"),
            CustomerNumber = "CUST-00002",
            Name = "Global Logistics Solutions",
            Email = "orders@globallogistics.com",
            Phone = "+1 (555) 876-5432",
            BillingAddress = "450 Harbor Blvd, San Francisco, CA 94105",
            ShippingAddress = "450 Harbor Blvd, San Francisco, CA 94105",
            CreditLimit = 25000.00m,
            IsActive = true
        };

        dbContext.Customers.AddRange(customer1, customer2);

        var order1 = new SalesOrder
        {
            Id = Guid.Parse("55555555-5555-5555-5555-111111111111"),
            OrderNumber = "SO-00001",
            CustomerId = customer1.Id,
            OrderDateUtc = DateTime.UtcNow.AddDays(-5),
            Status = OrderStatus.Shipped,
            TotalAmount = 22799.88m,
            Notes = "Urgent delivery for engineering team expansion"
        };

        order1.Lines.Add(new SalesOrderLine
        {
            ProductId = Guid.Parse("22222222-2222-2222-2222-111111111111"),
            Sku = "PRD-LAP-001",
            ProductName = "Enterprise Pro Laptop 16\"",
            Quantity = 12,
            UnitPrice = 1899.99m,
            DiscountPercentage = 0,
            LineTotal = 22799.88m
        });

        order1.Shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            SalesOrderId = order1.Id,
            TrackingNumber = "FEDEX-9876543210",
            Carrier = "FedEx Express",
            Status = ShipmentStatus.InTransit,
            ShippedAtUtc = DateTime.UtcNow.AddDays(-2)
        };

        dbContext.SalesOrders.Add(order1);

        await dbContext.SaveChangesAsync();
    }
}
