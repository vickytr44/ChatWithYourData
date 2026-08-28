using ChatWithYourData.SalesService.Application.Common;
using ChatWithYourData.SalesService.Application.Common.Interfaces;
using ChatWithYourData.SalesService.Application.Features.Sales.DTOs;
using ChatWithYourData.SalesService.Domain.Entities;
using ChatWithYourData.SalesService.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.SalesService.Application.Features.Sales.Commands;

public record CreateCustomerCommand(
    string Name,
    string Email,
    string Phone,
    string BillingAddress,
    string ShippingAddress,
    decimal CreditLimit
) : IRequest<Result<CustomerDto>>;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.CreditLimit).GreaterThanOrEqualTo(0);
    }
}

public class CreateCustomerCommandHandler(ISalesDbContext dbContext)
    : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await dbContext.Customers.AnyAsync(c => c.Email == request.Email, cancellationToken);
        if (emailExists)
            return Result<CustomerDto>.Failure($"Customer with email '{request.Email}' already exists.");

        var count = await dbContext.Customers.CountAsync(cancellationToken);
        var customer = new Customer
        {
            CustomerNumber = $"CUST-{(count + 1):D5}",
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            BillingAddress = request.BillingAddress,
            ShippingAddress = request.ShippingAddress,
            CreditLimit = request.CreditLimit,
            IsActive = true
        };

        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<CustomerDto>.Success(new CustomerDto(
            customer.Id,
            customer.CustomerNumber,
            customer.Name,
            customer.Email,
            customer.Phone,
            customer.BillingAddress,
            customer.ShippingAddress,
            customer.CreditLimit,
            customer.IsActive,
            customer.CreatedAtUtc
        ));
    }
}

public record CreateSalesOrderCommand(
    Guid CustomerId,
    string Notes,
    List<CreateSalesOrderLineInput> Lines
) : IRequest<Result<SalesOrderDto>>;

public class CreateSalesOrderCommandHandler(ISalesDbContext dbContext)
    : IRequestHandler<CreateSalesOrderCommand, Result<SalesOrderDto>>
{
    public async Task<Result<SalesOrderDto>> Handle(CreateSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.FindAsync([request.CustomerId], cancellationToken);
        if (customer == null)
            return Result<SalesOrderDto>.Failure($"Customer with ID {request.CustomerId} not found.");

        if (request.Lines == null || request.Lines.Count == 0)
            return Result<SalesOrderDto>.Failure("Sales order must contain at least one line item.");

        var count = await dbContext.SalesOrders.CountAsync(cancellationToken);
        var order = new SalesOrder
        {
            OrderNumber = $"SO-{(count + 1):D5}",
            CustomerId = request.CustomerId,
            OrderDateUtc = DateTime.UtcNow,
            Status = OrderStatus.Confirmed,
            Notes = request.Notes
        };

        decimal total = 0;
        foreach (var lineInput in request.Lines)
        {
            var lineTotal = lineInput.Quantity * lineInput.UnitPrice * (1 - lineInput.DiscountPercentage / 100m);
            total += lineTotal;

            order.Lines.Add(new SalesOrderLine
            {
                ProductId = lineInput.ProductId,
                Sku = lineInput.Sku,
                ProductName = lineInput.ProductName,
                Quantity = lineInput.Quantity,
                UnitPrice = lineInput.UnitPrice,
                DiscountPercentage = lineInput.DiscountPercentage,
                LineTotal = lineTotal
            });
        }
        order.TotalAmount = total;

        dbContext.SalesOrders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        var lineDtos = order.Lines.Select(l => new SalesOrderLineDto(
            l.Id,
            l.SalesOrderId,
            l.ProductId,
            l.Sku,
            l.ProductName,
            l.Quantity,
            l.UnitPrice,
            l.DiscountPercentage,
            l.LineTotal
        )).ToList();

        return Result<SalesOrderDto>.Success(new SalesOrderDto(
            order.Id,
            order.OrderNumber,
            order.CustomerId,
            order.OrderDateUtc,
            order.Status,
            order.TotalAmount,
            order.Notes,
            lineDtos
        ));
    }
}
