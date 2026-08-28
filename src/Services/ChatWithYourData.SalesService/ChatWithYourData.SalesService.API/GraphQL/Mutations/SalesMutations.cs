using ChatWithYourData.SalesService.API.GraphQL.Errors;
using ChatWithYourData.SalesService.Application.Features.Sales.Commands;
using ChatWithYourData.SalesService.Application.Features.Sales.DTOs;
using ChatWithYourData.SalesService.Domain.Entities;
using HotChocolate.Types;
using MediatR;

namespace ChatWithYourData.SalesService.API.GraphQL.Mutations;

[MutationType]
internal static partial class SalesMutations
{
    [Error(typeof(CustomerNumberAlreadyExistsException))]
    public static async Task<Customer> CreateCustomerAsync(
        string name,
        string email,
        string phone,
        string billingAddress,
        string shippingAddress,
        decimal creditLimit,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand(name, email, phone, billingAddress, shippingAddress, creditLimit);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            throw new CustomerNumberAlreadyExistsException(result.Error!);

        return new Customer
        {
            Id = result.Value!.Id,
            CustomerNumber = result.Value.CustomerNumber,
            Name = result.Value.Name,
            Email = result.Value.Email,
            Phone = result.Value.Phone,
            BillingAddress = result.Value.BillingAddress,
            ShippingAddress = result.Value.ShippingAddress,
            CreditLimit = result.Value.CreditLimit,
            IsActive = result.Value.IsActive
        };
    }

    [Error(typeof(CustomerNotFoundException))]
    public static async Task<SalesOrder> CreateSalesOrderAsync(
        Guid customerId,
        string notes,
        List<CreateSalesOrderLineInput> lines,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateSalesOrderCommand(customerId, notes, lines);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            throw new CustomerNotFoundException(customerId);

        return new SalesOrder
        {
            Id = result.Value!.Id,
            OrderNumber = result.Value.OrderNumber,
            CustomerId = result.Value.CustomerId,
            OrderDateUtc = result.Value.OrderDateUtc,
            Status = result.Value.Status,
            TotalAmount = result.Value.TotalAmount,
            Notes = result.Value.Notes
        };
    }
}
