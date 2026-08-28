using ChatWithYourData.FinanceService.Application.Features.Finance.Commands;
using ChatWithYourData.FinanceService.Application.Features.Finance.DTOs;
using ChatWithYourData.FinanceService.Domain.Enums;
using HotChocolate;
using HotChocolate.Types;
using MediatR;

namespace ChatWithYourData.FinanceService.API.GraphQL.Mutations;

public record CreateAccountInput(
    string AccountCode,
    string Name,
    AccountType Type,
    string Description,
    decimal InitialBalance
);

public record PostJournalEntryInput(
    string Description,
    string Reference,
    List<CreateJournalLineInput> Lines
);

public record FinanceMutationPayload<T>(bool Success, T? Data, string? Error);

public class FinanceMutations
{
    public async Task<FinanceMutationPayload<AccountDto>> CreateAccountAsync(
        CreateAccountInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateAccountCommand(
            input.AccountCode,
            input.Name,
            input.Type,
            input.Description,
            input.InitialBalance
        );

        var result = await mediator.Send(command, cancellationToken);
        return new FinanceMutationPayload<AccountDto>(result.IsSuccess, result.Value, result.Error);
    }

    public async Task<FinanceMutationPayload<JournalEntryDto>> PostJournalEntryAsync(
        PostJournalEntryInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new PostJournalEntryCommand(
            input.Description,
            input.Reference,
            input.Lines
        );

        var result = await mediator.Send(command, cancellationToken);
        return new FinanceMutationPayload<JournalEntryDto>(result.IsSuccess, result.Value, result.Error);
    }
}
