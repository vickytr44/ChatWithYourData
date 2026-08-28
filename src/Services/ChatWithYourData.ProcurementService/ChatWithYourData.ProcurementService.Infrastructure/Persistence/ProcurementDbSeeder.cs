using ChatWithYourData.ProcurementService.Domain.Entities;
using ChatWithYourData.ProcurementService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.ProcurementService.Infrastructure.Persistence;

public static class ProcurementDbSeeder
{
    public static async Task SeedAsync(ProcurementDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.Vendors.AnyAsync())
            return;

        var vendor1 = new Vendor
        {
            Id = Guid.Parse("66666666-6666-6666-6666-111111111111"),
            VendorCode = "VND-00001",
            Name = "Silicon Microdevices Inc",
            ContactEmail = "orders@siliconmicro.com",
            Phone = "+1 (555) 345-6789",
            Address = "200 Semiconductor Blvd, San Jose, CA 95134",
            PaymentTermsDays = 30,
            TaxId = "US-94-3829102",
            IsActive = true
        };

        var vendor2 = new Vendor
        {
            Id = Guid.Parse("66666666-6666-6666-6666-222222222222"),
            VendorCode = "VND-00002",
            Name = "Apex Display Technologies",
            ContactEmail = "sales@apexdisplays.com",
            Phone = "+1 (555) 765-4321",
            Address = "88 Optics Park, Austin, TX 78759",
            PaymentTermsDays = 45,
            TaxId = "US-74-9102843",
            IsActive = true
        };

        dbContext.Vendors.AddRange(vendor1, vendor2);

        var po1 = new PurchaseOrder
        {
            Id = Guid.Parse("77777777-7777-7777-7777-111111111111"),
            PoNumber = "PO-00001",
            VendorId = vendor1.Id,
            OrderDateUtc = DateTime.UtcNow.AddDays(-10),
            ExpectedDeliveryDateUtc = DateTime.UtcNow.AddDays(4),
            Status = PurchaseOrderStatus.Approved,
            TotalCost = 65000.00m,
            Notes = "Restock quarterly order for workstation components"
        };

        po1.Lines.Add(new PurchaseOrderLine
        {
            ProductId = Guid.Parse("22222222-2222-2222-2222-111111111111"),
            Sku = "PRD-LAP-001",
            ProductName = "Enterprise Pro Laptop 16\"",
            QuantityOrdered = 50,
            QuantityReceived = 0,
            UnitCost = 1300.00m,
            LineTotal = 65000.00m
        });

        dbContext.PurchaseOrders.Add(po1);

        await dbContext.SaveChangesAsync();
    }
}
