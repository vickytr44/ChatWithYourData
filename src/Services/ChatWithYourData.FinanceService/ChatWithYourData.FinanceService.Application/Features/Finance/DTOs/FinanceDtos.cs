using ChatWithYourData.FinanceService.Domain.Enums;

namespace ChatWithYourData.FinanceService.Application.Features.Finance.DTOs;

public record AccountDto(
    Guid Id,
    string AccountCode,
    string Name,
    AccountType Type,
    string Description,
    decimal CurrentBalance,
    bool IsActive,
    DateTime CreatedAtUtc
);

public record JournalLineDto(
    Guid Id,
    Guid JournalEntryId,
    Guid AccountId,
    decimal DebitAmount,
    decimal CreditAmount,
    string Memo
);

public record JournalEntryDto(
    Guid Id,
    string EntryNumber,
    DateTime EntryDateUtc,
    string Description,
    string Reference,
    bool IsPosted,
    IReadOnlyList<JournalLineDto> Lines
);

public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    Guid? CustomerId,
    Guid? SalesOrderId,
    DateTime IssueDateUtc,
    DateTime DueDateUtc,
    decimal Subtotal,
    decimal TaxAmount,
    decimal TotalAmount,
    decimal PaidAmount,
    InvoiceStatus Status,
    string Notes
);

public record PaymentDto(
    Guid Id,
    Guid InvoiceId,
    string PaymentNumber,
    DateTime PaymentDateUtc,
    decimal Amount,
    PaymentMethod Method,
    string ReferenceNumber
);

public record CreateJournalLineInput(
    Guid AccountId,
    decimal DebitAmount,
    decimal CreditAmount,
    string Memo
);
