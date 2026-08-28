using ChatWithYourData.FinanceService.API.GraphQL.DataLoaders;
using ChatWithYourData.FinanceService.Domain.Entities;
using GreenDonut.Data;
using HotChocolate;
using HotChocolate.Types;

namespace ChatWithYourData.FinanceService.API.GraphQL.Types;

[GraphQLName("Customer")]
public class CustomerEntityStub
{
    public Guid Id { get; set; }
}

[ObjectType<CustomerEntityStub>]
internal static partial class CustomerEntityStubNode
{
    static partial void Configure(IObjectTypeDescriptor<CustomerEntityStub> descriptor)
    {
        descriptor.Field(c => c.Id).Type<NonNullType<IdType>>();
    }
}

[ObjectType<JournalEntry>]
internal static partial class JournalEntryNode
{
    public static async Task<List<JournalLine>> GetLinesAsync(
        [Parent(requires: nameof(JournalEntry.Id))] JournalEntry entry,
        QueryContext<JournalLine> query,
        JournalLinesByEntryIdDataLoader journalLinesByEntryId,
        CancellationToken cancellationToken)
        => await journalLinesByEntryId.With(query).LoadAsync(entry.Id, cancellationToken) ?? [];

    static partial void Configure(IObjectTypeDescriptor<JournalEntry> descriptor)
    {
        descriptor.Ignore(j => j.Lines);
    }
}

[ObjectType<Invoice>]
internal static partial class InvoiceNode
{
    public static CustomerEntityStub? GetCustomer(
        [Parent(requires: nameof(Invoice.CustomerId))] Invoice invoice)
        => invoice.CustomerId.HasValue ? new CustomerEntityStub { Id = invoice.CustomerId.Value } : null;

    public static async Task<List<Payment>> GetPaymentsAsync(
        [Parent(requires: nameof(Invoice.Id))] Invoice invoice,
        QueryContext<Payment> query,
        PaymentsByInvoiceIdDataLoader paymentsByInvoiceId,
        CancellationToken cancellationToken)
        => await paymentsByInvoiceId.With(query).LoadAsync(invoice.Id, cancellationToken) ?? [];

    static partial void Configure(IObjectTypeDescriptor<Invoice> descriptor)
    {
        descriptor.Ignore(i => i.Payments);
    }
}

[ObjectType<Account>]
internal static partial class AccountNode
{
    static partial void Configure(IObjectTypeDescriptor<Account> descriptor)
    {
        descriptor.Ignore(a => a.JournalLines);
    }
}

[ObjectType<JournalLine>]
internal static partial class JournalLineNode
{
    public static async Task<Account?> GetAccountAsync(
        [Parent(requires: nameof(JournalLine.AccountId))] JournalLine line,
        QueryContext<Account> query,
        AccountByIdDataLoader accountById,
        CancellationToken cancellationToken)
        => await accountById.With(query).LoadAsync(line.AccountId, cancellationToken);

    static partial void Configure(IObjectTypeDescriptor<JournalLine> descriptor)
    {
        descriptor.Ignore(l => l.JournalEntry);
        descriptor.Ignore(l => l.Account);
    }
}

[ObjectType<Payment>]
internal static partial class PaymentNode
{
    public static async Task<Invoice?> GetInvoiceAsync(
        [Parent(requires: nameof(Payment.InvoiceId))] Payment payment,
        QueryContext<Invoice> query,
        InvoiceByIdDataLoader invoiceById,
        CancellationToken cancellationToken)
        => await invoiceById.With(query).LoadAsync(payment.InvoiceId, cancellationToken);

    static partial void Configure(IObjectTypeDescriptor<Payment> descriptor)
    {
        descriptor.Ignore(p => p.Invoice);
    }
}
