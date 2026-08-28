using ChatWithYourData.FinanceService.API.GraphQL.DataLoaders;
using ChatWithYourData.FinanceService.Domain.Entities;
using ChatWithYourData.FinanceService.Infrastructure.Persistence;
using GreenDonut.Data;
using HotChocolate.Data;
using HotChocolate.Types;
using HotChocolate.Types.Pagination;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.FinanceService.API.GraphQL.Queries;

[QueryType]
internal static partial class FinanceQueries
{
    [UseFiltering]
    [UseSorting]
    public static async Task<PageConnection<Account>> GetAccountsAsync(
        PagingArguments pagingArguments,
        QueryContext<Account> query,
        FinanceDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.Accounts
            .AsNoTracking()
            .OrderBy(a => a.AccountCode)
            .ThenBy(a => a.Id)
            .With(query)
            .ToPageAsync(pagingArguments, cancellationToken);

    [Lookup]
    public static async Task<Account?> GetAccountByIdAsync(
        Guid id,
        QueryContext<Account> query,
        AccountByIdDataLoader accountById,
        CancellationToken cancellationToken)
        => await accountById.With(query).LoadAsync(id, cancellationToken);

    [Lookup]
    public static async Task<Invoice?> GetInvoiceByIdAsync(
        Guid id,
        QueryContext<Invoice> query,
        InvoiceByIdDataLoader invoiceById,
        CancellationToken cancellationToken)
        => await invoiceById.With(query).LoadAsync(id, cancellationToken);

    [UseFiltering]
    [UseSorting]
    public static async Task<PageConnection<JournalEntry>> GetJournalEntriesAsync(
        PagingArguments pagingArguments,
        QueryContext<JournalEntry> query,
        FinanceDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.JournalEntries
            .AsNoTracking()
            .OrderByDescending(j => j.EntryDateUtc)
            .ThenBy(j => j.Id)
            .With(query)
            .ToPageAsync(pagingArguments, cancellationToken);

    [Lookup]
    public static async Task<JournalEntry?> GetJournalEntryByIdAsync(
        Guid id,
        QueryContext<JournalEntry> query,
        JournalEntryByIdDataLoader journalEntryById,
        CancellationToken cancellationToken)
        => await journalEntryById.With(query).LoadAsync(id, cancellationToken);

    [UseFiltering]
    [UseSorting]
    public static async Task<PageConnection<Invoice>> GetInvoicesAsync(
        PagingArguments pagingArguments,
        QueryContext<Invoice> query,
        FinanceDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.Invoices
            .AsNoTracking()
            .OrderByDescending(i => i.IssueDateUtc)
            .ThenBy(i => i.Id)
            .With(query)
            .ToPageAsync(pagingArguments, cancellationToken);

    [UseFiltering]
    [UseSorting]
    public static async Task<PageConnection<Payment>> GetPaymentsAsync(
        PagingArguments pagingArguments,
        QueryContext<Payment> query,
        FinanceDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.Payments
            .AsNoTracking()
            .OrderByDescending(p => p.PaymentDateUtc)
            .ThenBy(p => p.Id)
            .With(query)
            .ToPageAsync(pagingArguments, cancellationToken);

    [Lookup]
    public static async Task<Payment?> GetPaymentByIdAsync(
        Guid id,
        QueryContext<Payment> query,
        PaymentByIdDataLoader paymentById,
        CancellationToken cancellationToken)
        => await paymentById.With(query).LoadAsync(id, cancellationToken);
}
