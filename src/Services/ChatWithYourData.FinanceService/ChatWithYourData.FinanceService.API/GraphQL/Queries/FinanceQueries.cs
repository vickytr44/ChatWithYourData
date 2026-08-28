using ChatWithYourData.FinanceService.API.GraphQL.DataLoaders;
using ChatWithYourData.FinanceService.Domain.Entities;
using ChatWithYourData.FinanceService.Infrastructure.Persistence;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.FinanceService.API.GraphQL.Queries;

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
    public async Task<Account?> GetAccountByIdAsync(
        Guid id,
        AccountByIdDataLoader dataLoader,
        CancellationToken cancellationToken)
    {
        return await dataLoader.LoadAsync(id, cancellationToken);
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
