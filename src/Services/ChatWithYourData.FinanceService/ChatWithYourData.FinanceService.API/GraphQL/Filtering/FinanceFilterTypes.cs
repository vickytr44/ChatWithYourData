using ChatWithYourData.FinanceService.Domain.Entities;
using HotChocolate.Data.Filters;

namespace ChatWithYourData.FinanceService.API.GraphQL.Filtering;

public sealed class AccountFilterInputType : FilterInputType<Account>
{
    protected override void Configure(IFilterInputTypeDescriptor<Account> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.AccountCode);
        descriptor.Field(t => t.Name);
        descriptor.Field(t => t.Type);
        descriptor.Field(t => t.IsActive);
    }
}

public sealed class JournalEntryFilterInputType : FilterInputType<JournalEntry>
{
    protected override void Configure(IFilterInputTypeDescriptor<JournalEntry> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.EntryNumber);
        descriptor.Field(t => t.EntryDateUtc);
        descriptor.Field(t => t.IsPosted);
    }
}

public sealed class InvoiceFilterInputType : FilterInputType<Invoice>
{
    protected override void Configure(IFilterInputTypeDescriptor<Invoice> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.InvoiceNumber);
        descriptor.Field(t => t.CustomerId);
        descriptor.Field(t => t.Status);
        descriptor.Field(t => t.DueDateUtc);
    }
}

public sealed class PaymentFilterInputType : FilterInputType<Payment>
{
    protected override void Configure(IFilterInputTypeDescriptor<Payment> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.InvoiceId);
        descriptor.Field(t => t.PaymentNumber);
        descriptor.Field(t => t.Method);
    }
}
