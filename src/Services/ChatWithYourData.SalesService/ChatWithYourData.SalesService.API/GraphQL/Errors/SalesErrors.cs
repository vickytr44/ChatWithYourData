namespace ChatWithYourData.SalesService.API.GraphQL.Errors;

public sealed class CustomerNotFoundException(Guid customerId)
    : Exception($"Customer with ID {customerId} was not found.")
{
    public Guid CustomerId { get; } = customerId;
}

public sealed class CustomerNumberAlreadyExistsException(string customerNumber)
    : Exception($"Customer with number '{customerNumber}' already exists.")
{
    public string CustomerNumber { get; } = customerNumber;
}

public sealed class SalesOrderNotFoundException(Guid orderId)
    : Exception($"Sales order with ID {orderId} was not found.")
{
    public Guid OrderId { get; } = orderId;
}
