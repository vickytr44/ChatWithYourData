using ChatWithYourData.SalesService.Application.Features.Sales.Commands;
using ChatWithYourData.SalesService.Application.Features.Sales.DTOs;
using HotChocolate;
using HotChocolate.Types;
using MediatR;

namespace ChatWithYourData.SalesService.API.GraphQL.Mutations;

public record CreateCustomerInput(
    string Name,
    string Email,
    string Phone,
    string BillingAddress,
    string ShippingAddress,
    decimal CreditLimit
);

public record CreateSalesOrderInput(
    Guid CustomerId,
    string Notes,
    List<CreateSalesOrderLineInput> Lines
);

public record SalesMutationPayload<T>(bool Success, T? Data, string? Error);

[GraphQLName("Mutation")]
public class SalesMutations
{
    public async Task<SalesMutationPayload<CustomerDto>> CreateCustomerAsync(
        CreateCustomerInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand(
            input.Name,
            input.Email,
            input.Phone,
            input.BillingAddress,
            input.ShippingAddress,
            input.CreditLimit
        );

        var result = await mediator.Send(command, cancellationToken);
        return new SalesMutationPayload<CustomerDto>(result.IsSuccess, result.Value, result.Error);
    }

    public async Task<SalesMutationPayload<SalesOrderDto>> CreateSalesOrderAsync(
        CreateSalesOrderInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateSalesOrderCommand(
            input.CustomerId,
            input.Notes,
            input.Lines
        );

        var result = await mediator.Send(command, cancellationToken);
        return new SalesMutationPayload<SalesOrderDto>(result.IsSuccess, result.Value, result.Error);
    }
}
