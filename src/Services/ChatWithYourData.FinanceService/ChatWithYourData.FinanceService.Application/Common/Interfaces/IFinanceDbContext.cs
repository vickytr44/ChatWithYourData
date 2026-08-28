using ChatWithYourData.FinanceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.FinanceService.Application.Common.Interfaces;

public interface IFinanceDbContext
{
    DbSet<Account> Accounts { get; }
    DbSet<JournalEntry> JournalEntries { get; }
    DbSet<JournalLine> JournalLines { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<Payment> Payments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
