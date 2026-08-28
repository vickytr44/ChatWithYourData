using ChatWithYourData.FinanceService.API.GraphQL.Errors;
using ChatWithYourData.FinanceService.Application.Features.Finance.Commands;
using ChatWithYourData.FinanceService.Application.Features.Finance.DTOs;
using ChatWithYourData.FinanceService.Domain.Entities;
using ChatWithYourData.FinanceService.Domain.Enums;
using HotChocolate.Types;
using MediatR;

namespace ChatWithYourData.FinanceService.API.GraphQL.Mutations;

[MutationType]
internal static partial class FinanceMutations
{
    [Error(typeof(AccountCodeAlreadyExistsException))]
    public static async Task<Account> CreateAccountAsync(
        string accountCode,
        string name,
        AccountType type,
        string description,
        decimal initialBalance,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateAccountCommand(accountCode, name, type, description, initialBalance);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            throw new AccountCodeAlreadyExistsException(accountCode);

        return new Account
        {
            Id = result.Value!.Id,
            AccountCode = result.Value.AccountCode,
            Name = result.Value.Name,
            Type = result.Value.Type,
            Description = result.Value.Description,
            CurrentBalance = result.Value.CurrentBalance,
            IsActive = result.Value.IsActive
        };
    }

    [Error(typeof(UnbalancedJournalEntryException))]
    public static async Task<JournalEntry> PostJournalEntryAsync(
        string description,
        string reference,
        List<CreateJournalLineInput> lines,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new PostJournalEntryCommand(description, reference, lines);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            var debits = lines?.Sum(l => l.DebitAmount) ?? 0;
            var credits = lines?.Sum(l => l.CreditAmount) ?? 0;
            throw new UnbalancedJournalEntryException(debits, credits);
        }

        return new JournalEntry
        {
            Id = result.Value!.Id,
            EntryNumber = result.Value.EntryNumber,
            EntryDateUtc = result.Value.EntryDateUtc,
            Description = result.Value.Description,
            Reference = result.Value.Reference,
            IsPosted = result.Value.IsPosted
        };
    }
}
