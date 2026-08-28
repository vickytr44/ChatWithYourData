using ChatWithYourData.FinanceService.Domain.Common;
using ChatWithYourData.FinanceService.Domain.Enums;

namespace ChatWithYourData.FinanceService.Domain.Entities;

public class Account : BaseEntity
{
    public string AccountCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<JournalLine> JournalLines { get; set; } = new List<JournalLine>();
}

public class JournalEntry : BaseEntity
{
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDateUtc { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public bool IsPosted { get; set; } = true;

    public ICollection<JournalLine> Lines { get; set; } = new List<JournalLine>();
}

public class JournalLine : BaseEntity
{
    public Guid JournalEntryId { get; set; }
    public Guid AccountId { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string Memo { get; set; } = string.Empty;

    public JournalEntry? JournalEntry { get; set; }
    public Account? Account { get; set; }
}

public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public Guid? SalesOrderId { get; set; }
    public DateTime IssueDateUtc { get; set; } = DateTime.UtcNow;
    public DateTime DueDateUtc { get; set; } = DateTime.UtcNow.AddDays(30);
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Issued;
    public string Notes { get; set; } = string.Empty;

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public class Payment : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public DateTime PaymentDateUtc { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; } = PaymentMethod.BankTransfer;
    public string ReferenceNumber { get; set; } = string.Empty;

    public Invoice? Invoice { get; set; }
}
