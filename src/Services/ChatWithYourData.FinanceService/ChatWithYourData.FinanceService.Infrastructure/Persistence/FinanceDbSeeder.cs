using ChatWithYourData.FinanceService.Domain.Entities;
using ChatWithYourData.FinanceService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.FinanceService.Infrastructure.Persistence;

public static class FinanceDbSeeder
{
    public static async Task SeedAsync(FinanceDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.Accounts.AnyAsync())
            return;

        var cash = new Account { Id = Guid.Parse("88888888-8888-8888-8888-111111111111"), AccountCode = "1010", Name = "Cash and Cash Equivalents", Type = AccountType.Asset, CurrentBalance = 150000.00m };
        var ar = new Account { Id = Guid.Parse("88888888-8888-8888-8888-222222222222"), AccountCode = "1100", Name = "Accounts Receivable", Type = AccountType.Asset, CurrentBalance = 22799.88m };
        var inventory = new Account { Id = Guid.Parse("88888888-8888-8888-8888-333333333333"), AccountCode = "1200", Name = "Merchandise Inventory", Type = AccountType.Asset, CurrentBalance = 85000.00m };
        var ap = new Account { Id = Guid.Parse("88888888-8888-8888-8888-444444444444"), AccountCode = "2010", Name = "Accounts Payable", Type = AccountType.Liability, CurrentBalance = 65000.00m };
        var revenue = new Account { Id = Guid.Parse("88888888-8888-8888-8888-555555555555"), AccountCode = "4010", Name = "Product Sales Revenue", Type = AccountType.Revenue, CurrentBalance = 22799.88m };
        var cogs = new Account { Id = Guid.Parse("88888888-8888-8888-8888-666666666666"), AccountCode = "5010", Name = "Cost of Goods Sold", Type = AccountType.Expense, CurrentBalance = 15600.00m };

        dbContext.Accounts.AddRange(cash, ar, inventory, ap, revenue, cogs);

        var invoice1 = new Invoice
        {
            Id = Guid.Parse("99999999-9999-9999-9999-111111111111"),
            InvoiceNumber = "INV-00001",
            CustomerId = Guid.Parse("44444444-4444-4444-4444-111111111111"),
            SalesOrderId = Guid.Parse("55555555-5555-5555-5555-111111111111"),
            IssueDateUtc = DateTime.UtcNow.AddDays(-3),
            DueDateUtc = DateTime.UtcNow.AddDays(27),
            Subtotal = 22799.88m,
            TaxAmount = 0m,
            TotalAmount = 22799.88m,
            PaidAmount = 10000.00m,
            Status = InvoiceStatus.PartiallyPaid,
            Notes = "First installment received for SO-00001"
        };

        invoice1.Payments.Add(new Payment
        {
            InvoiceId = invoice1.Id,
            PaymentNumber = "PAY-00001",
            PaymentDateUtc = DateTime.UtcNow.AddDays(-1),
            Amount = 10000.00m,
            Method = PaymentMethod.BankTransfer,
            ReferenceNumber = "WIRE-TX-549102"
        });

        dbContext.Invoices.Add(invoice1);

        await dbContext.SaveChangesAsync();
    }
}
