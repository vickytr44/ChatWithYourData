using ChatWithYourData.FinanceService.API.GraphQL.DataLoaders;
using ChatWithYourData.FinanceService.Domain.Entities;
using HotChocolate.Types;

namespace ChatWithYourData.FinanceService.API.GraphQL.Types;

public class JournalEntryType : ObjectType<JournalEntry>
{
    protected override void Configure(IObjectTypeDescriptor<JournalEntry> descriptor)
    {
        descriptor.Description("Represents a posted or draft journal entry in the general ledger.");

        descriptor.Field(j => j.Lines)
            .ResolveWith<JournalEntryResolvers>(r => r.GetLinesAsync(default!, default!, default!))
            .Description("The debit and credit lines for this entry (resolved via DataLoader).");
    }

    private class JournalEntryResolvers
    {
        public async Task<List<JournalLine>> GetLinesAsync(
            [Parent] JournalEntry entry,
            JournalLinesByEntryIdDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            var lines = await dataLoader.LoadAsync(entry.Id, cancellationToken);
            return lines ?? new List<JournalLine>();
        }
    }
}

public class InvoiceType : ObjectType<Invoice>
{
    protected override void Configure(IObjectTypeDescriptor<Invoice> descriptor)
    {
        descriptor.Description("Represents a customer invoice.");

        descriptor.Field(i => i.Payments)
            .ResolveWith<InvoiceResolvers>(r => r.GetPaymentsAsync(default!, default!, default!))
            .Description("The payments applied to this invoice (resolved via DataLoader).");
    }

    private class InvoiceResolvers
    {
        public async Task<List<Payment>> GetPaymentsAsync(
            [Parent] Invoice invoice,
            PaymentsByInvoiceIdDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            var payments = await dataLoader.LoadAsync(invoice.Id, cancellationToken);
            return payments ?? new List<Payment>();
        }
    }
}
