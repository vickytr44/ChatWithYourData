using ChatWithYourData.SalesService.Domain.Enums;

namespace ChatWithYourData.SalesService.Application.Features.Sales.DTOs;

public record CustomerDto(
    Guid Id,
    string CustomerNumber,
    string Name,
    string Email,
    string Phone,
    string BillingAddress,
    string ShippingAddress,
    decimal CreditLimit,
    bool IsActive,
    DateTime CreatedAtUtc
);

public record SalesOrderLineDto(
    Guid Id,
    Guid SalesOrderId,
    Guid ProductId,
    string Sku,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountPercentage,
    decimal LineTotal
);

public record SalesOrderDto(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    DateTime OrderDateUtc,
    OrderStatus Status,
    decimal TotalAmount,
    string Notes,
    IReadOnlyList<SalesOrderLineDto> Lines
);

public record CreateSalesOrderLineInput(
    Guid ProductId,
    string Sku,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountPercentage
);
