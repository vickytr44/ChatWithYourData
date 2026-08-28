using ChatWithYourData.FinanceService.Domain.Entities;
using ChatWithYourData.FinanceService.Infrastructure.Persistence;
using GreenDonut.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.FinanceService.API.GraphQL.DataLoaders;

internal static class FinanceDataLoaders
{
    [DataLoader]
    public static async Task<Dictionary<Guid, Account>> GetAccountByIdAsync(
        IReadOnlyList<Guid> ids,
        QueryContext<Account> query,
        FinanceDbContext context,
        CancellationToken cancellationToken)
        => await context.Accounts
            .AsNoTracking()
            .Where(a => ids.Contains(a.Id))
            .With(query.Include(a => a.Id))
            .ToDictionaryAsync(a => a.Id, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<Guid, Invoice>> GetInvoiceByIdAsync(
        IReadOnlyList<Guid> ids,
        QueryContext<Invoice> query,
        FinanceDbContext context,
        CancellationToken cancellationToken)
        => await context.Invoices
            .AsNoTracking()
            .Where(i => ids.Contains(i.Id))
            .With(query.Include(i => i.Id))
            .ToDictionaryAsync(i => i.Id, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<Guid, JournalEntry>> GetJournalEntryByIdAsync(
        IReadOnlyList<Guid> ids,
        QueryContext<JournalEntry> query,
        FinanceDbContext context,
        CancellationToken cancellationToken)
        => await context.JournalEntries
            .AsNoTracking()
            .Where(j => ids.Contains(j.Id))
            .With(query.Include(j => j.Id))
            .ToDictionaryAsync(j => j.Id, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<Guid, Payment>> GetPaymentByIdAsync(
        IReadOnlyList<Guid> ids,
        QueryContext<Payment> query,
        FinanceDbContext context,
        CancellationToken cancellationToken)
        => await context.Payments
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .With(query.Include(p => p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

    [DataLoader]
    public static async Task<Dictionary<Guid, List<JournalLine>>> GetJournalLinesByEntryIdAsync(
        IReadOnlyList<Guid> entryIds,
        QueryContext<JournalLine> query,
        FinanceDbContext context,
        CancellationToken cancellationToken)
        => (await context.JournalLines
            .AsNoTracking()
            .Where(l => entryIds.Contains(l.JournalEntryId))
            .With(query.Include(l => l.JournalEntryId))
            .ToListAsync(cancellationToken))
            .GroupBy(l => l.JournalEntryId)
            .ToDictionary(g => g.Key, g => g.ToList());

    [DataLoader]
    public static async Task<Dictionary<Guid, List<Payment>>> GetPaymentsByInvoiceIdAsync(
        IReadOnlyList<Guid> invoiceIds,
        QueryContext<Payment> query,
        FinanceDbContext context,
        CancellationToken cancellationToken)
        => (await context.Payments
            .AsNoTracking()
            .Where(p => invoiceIds.Contains(p.InvoiceId))
            .With(query.Include(p => p.InvoiceId))
            .ToListAsync(cancellationToken))
            .GroupBy(p => p.InvoiceId)
            .ToDictionary(g => g.Key, g => g.ToList());
}
