using ChatWithYourData.FinanceService.Domain.Entities;
using ChatWithYourData.FinanceService.Infrastructure.Persistence;
using GreenDonut;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.FinanceService.API.GraphQL.DataLoaders;

public class AccountByIdDataLoader(
    FinanceDbContext dbContext,
    IBatchScheduler batchScheduler,
    DataLoaderOptions? options = null)
    : BatchDataLoader<Guid, Account>(batchScheduler, options ?? new DataLoaderOptions())
{
    protected override async Task<IReadOnlyDictionary<Guid, Account>> LoadBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken)
    {
        return await dbContext.Accounts
            .AsNoTracking()
            .Where(a => keys.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, cancellationToken);
    }
}

public class JournalLinesByEntryIdDataLoader(
    FinanceDbContext dbContext,
    IBatchScheduler batchScheduler,
    DataLoaderOptions? options = null)
    : BatchDataLoader<Guid, List<JournalLine>>(batchScheduler, options ?? new DataLoaderOptions())
{
    protected override async Task<IReadOnlyDictionary<Guid, List<JournalLine>>> LoadBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken)
    {
        var lines = await dbContext.JournalLines
            .AsNoTracking()
            .Where(l => keys.Contains(l.JournalEntryId))
            .ToListAsync(cancellationToken);

        return lines.GroupBy(l => l.JournalEntryId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}

public class PaymentsByInvoiceIdDataLoader(
    FinanceDbContext dbContext,
    IBatchScheduler batchScheduler,
    DataLoaderOptions? options = null)
    : BatchDataLoader<Guid, List<Payment>>(batchScheduler, options ?? new DataLoaderOptions())
{
    protected override async Task<IReadOnlyDictionary<Guid, List<Payment>>> LoadBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken)
    {
        var payments = await dbContext.Payments
            .AsNoTracking()
            .Where(p => keys.Contains(p.InvoiceId))
            .ToListAsync(cancellationToken);

        return payments.GroupBy(p => p.InvoiceId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
