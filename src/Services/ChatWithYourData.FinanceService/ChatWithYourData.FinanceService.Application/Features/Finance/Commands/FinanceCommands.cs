using ChatWithYourData.FinanceService.Application.Common;
using ChatWithYourData.FinanceService.Application.Common.Interfaces;
using ChatWithYourData.FinanceService.Application.Features.Finance.DTOs;
using ChatWithYourData.FinanceService.Domain.Entities;
using ChatWithYourData.FinanceService.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.FinanceService.Application.Features.Finance.Commands;

public record CreateAccountCommand(
    string AccountCode,
    string Name,
    AccountType Type,
    string Description,
    decimal InitialBalance
) : IRequest<Result<AccountDto>>;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.AccountCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public class CreateAccountCommandHandler(IFinanceDbContext dbContext)
    : IRequestHandler<CreateAccountCommand, Result<AccountDto>>
{
    public async Task<Result<AccountDto>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var codeExists = await dbContext.Accounts.AnyAsync(a => a.AccountCode == request.AccountCode, cancellationToken);
        if (codeExists)
            return Result<AccountDto>.Failure($"Account code '{request.AccountCode}' already exists.");

        var account = new Account
        {
            AccountCode = request.AccountCode,
            Name = request.Name,
            Type = request.Type,
            Description = request.Description,
            CurrentBalance = request.InitialBalance,
            IsActive = true
        };

        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<AccountDto>.Success(new AccountDto(
            account.Id,
            account.AccountCode,
            account.Name,
            account.Type,
            account.Description,
            account.CurrentBalance,
            account.IsActive,
            account.CreatedAtUtc
        ));
    }
}

public record PostJournalEntryCommand(
    string Description,
    string Reference,
    List<CreateJournalLineInput> Lines
) : IRequest<Result<JournalEntryDto>>;

public class PostJournalEntryCommandHandler(IFinanceDbContext dbContext)
    : IRequestHandler<PostJournalEntryCommand, Result<JournalEntryDto>>
{
    public async Task<Result<JournalEntryDto>> Handle(PostJournalEntryCommand request, CancellationToken cancellationToken)
    {
        if (request.Lines == null || request.Lines.Count < 2)
            return Result<JournalEntryDto>.Failure("A journal entry requires at least two lines for double-entry bookkeeping.");

        var totalDebit = request.Lines.Sum(l => l.DebitAmount);
        var totalCredit = request.Lines.Sum(l => l.CreditAmount);

        if (totalDebit != totalCredit)
            return Result<JournalEntryDto>.Failure($"Total debits ({totalDebit}) must equal total credits ({totalCredit}).");

        var count = await dbContext.JournalEntries.CountAsync(cancellationToken);
        var entry = new JournalEntry
        {
            EntryNumber = $"JE-{(count + 1):D5}",
            EntryDateUtc = DateTime.UtcNow,
            Description = request.Description,
            Reference = request.Reference,
            IsPosted = true
        };

        foreach (var lineInput in request.Lines)
        {
            var account = await dbContext.Accounts.FindAsync([lineInput.AccountId], cancellationToken);
            if (account == null)
                return Result<JournalEntryDto>.Failure($"Account ID {lineInput.AccountId} not found.");

            // Update account balance based on normal balance rules
            if (account.Type is AccountType.Asset or AccountType.Expense)
                account.CurrentBalance += (lineInput.DebitAmount - lineInput.CreditAmount);
            else
                account.CurrentBalance += (lineInput.CreditAmount - lineInput.DebitAmount);

            entry.Lines.Add(new JournalLine
            {
                AccountId = lineInput.AccountId,
                DebitAmount = lineInput.DebitAmount,
                CreditAmount = lineInput.CreditAmount,
                Memo = lineInput.Memo
            });
        }

        dbContext.JournalEntries.Add(entry);
        await dbContext.SaveChangesAsync(cancellationToken);

        var lineDtos = entry.Lines.Select(l => new JournalLineDto(
            l.Id,
            l.JournalEntryId,
            l.AccountId,
            l.DebitAmount,
            l.CreditAmount,
            l.Memo
        )).ToList();

        return Result<JournalEntryDto>.Success(new JournalEntryDto(
            entry.Id,
            entry.EntryNumber,
            entry.EntryDateUtc,
            entry.Description,
            entry.Reference,
            entry.IsPosted,
            lineDtos
        ));
    }
}
