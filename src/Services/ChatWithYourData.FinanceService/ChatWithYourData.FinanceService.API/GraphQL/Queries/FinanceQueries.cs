using ChatWithYourData.FinanceService.API.GraphQL.DataLoaders;
using ChatWithYourData.FinanceService.Domain.Entities;
using ChatWithYourData.FinanceService.Infrastructure.Persistence;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.FinanceService.API.GraphQL.Queries;

[GraphQLName("Query")]
public class FinanceQueries
{
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Account> GetAccounts(FinanceDbContext dbContext)
    {
        return dbContext.Accounts.AsNoTracking();
    }

    [UseProjection]
    [Lookup]
    public async Task<Account?> GetAccountByIdAsync(
        Guid id,
        AccountByIdDataLoader dataLoader,
        CancellationToken cancellationToken)
    {
        return await dataLoader.LoadAsync(id, cancellationToken);
    }

    [UseProjection]
    [Lookup]
    public async Task<Invoice?> GetInvoiceByIdAsync(
        Guid id,
        FinanceDbContext dbContext,
        CancellationToken cancellationToken)
    {
        return await dbContext.Invoices.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<JournalEntry> GetJournalEntries(FinanceDbContext dbContext)
    {
        return dbContext.JournalEntries.AsNoTracking();
    }

    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Invoice> GetInvoices(FinanceDbContext dbContext)
    {
        return dbContext.Invoices.AsNoTracking();
    }

    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Payment> GetPayments(FinanceDbContext dbContext)
    {
        return dbContext.Payments.AsNoTracking();
    }
}
