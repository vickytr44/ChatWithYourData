namespace ChatWithYourData.FinanceService.API.GraphQL.Errors;

public sealed class AccountCodeAlreadyExistsException(string accountCode)
    : Exception($"Account with code '{accountCode}' already exists.")
{
    public string AccountCode { get; } = accountCode;
}

public sealed class AccountNotFoundException(Guid accountId)
    : Exception($"Account with ID {accountId} was not found.")
{
    public Guid AccountId { get; } = accountId;
}

public sealed class UnbalancedJournalEntryException(decimal totalDebits, decimal totalCredits)
    : Exception($"Journal entry debits ({totalDebits}) do not equal credits ({totalCredits}).")
{
    public decimal TotalDebits { get; } = totalDebits;
    public decimal TotalCredits { get; } = totalCredits;
}

public sealed class InvoiceNotFoundException(Guid invoiceId)
    : Exception($"Invoice with ID {invoiceId} was not found.")
{
    public Guid InvoiceId { get; } = invoiceId;
}
