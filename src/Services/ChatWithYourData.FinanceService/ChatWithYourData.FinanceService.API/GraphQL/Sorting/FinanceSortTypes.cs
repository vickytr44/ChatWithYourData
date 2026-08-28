using ChatWithYourData.FinanceService.Domain.Entities;
using HotChocolate.Data.Sorting;

namespace ChatWithYourData.FinanceService.API.GraphQL.Sorting;

public sealed class AccountSortInputType : SortInputType<Account>
{
    protected override void Configure(ISortInputTypeDescriptor<Account> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.AccountCode);
        descriptor.Field(t => t.Name);
        descriptor.Field(t => t.CurrentBalance);
    }
}

public sealed class JournalEntrySortInputType : SortInputType<JournalEntry>
{
    protected override void Configure(ISortInputTypeDescriptor<JournalEntry> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.EntryNumber);
        descriptor.Field(t => t.EntryDateUtc);
    }
}

public sealed class InvoiceSortInputType : SortInputType<Invoice>
{
    protected override void Configure(ISortInputTypeDescriptor<Invoice> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.InvoiceNumber);
        descriptor.Field(t => t.IssueDateUtc);
        descriptor.Field(t => t.TotalAmount);
    }
}

public sealed class PaymentSortInputType : SortInputType<Payment>
{
    protected override void Configure(ISortInputTypeDescriptor<Payment> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.PaymentNumber);
        descriptor.Field(t => t.PaymentDateUtc);
        descriptor.Field(t => t.Amount);
    }
}
