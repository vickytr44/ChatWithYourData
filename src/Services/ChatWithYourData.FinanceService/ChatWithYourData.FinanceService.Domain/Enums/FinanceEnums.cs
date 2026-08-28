namespace ChatWithYourData.FinanceService.Domain.Enums;

public enum AccountType
{
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Revenue = 4,
    Expense = 5
}

public enum InvoiceStatus
{
    Draft = 1,
    Issued = 2,
    PartiallyPaid = 3,
    Paid = 4,
    Overdue = 5,
    Voided = 6
}

public enum PaymentMethod
{
    BankTransfer = 1,
    CreditCard = 2,
    Check = 3,
    Wire = 4,
    Cash = 5
}
