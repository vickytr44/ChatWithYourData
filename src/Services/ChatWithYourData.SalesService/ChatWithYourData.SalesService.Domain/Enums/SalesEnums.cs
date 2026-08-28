namespace ChatWithYourData.SalesService.Domain.Enums;

public enum OrderStatus
{
    Draft = 1,
    Confirmed = 2,
    Processing = 3,
    Shipped = 4,
    Completed = 5,
    Cancelled = 6
}

public enum ShipmentStatus
{
    Pending = 1,
    InTransit = 2,
    Delivered = 3,
    Returned = 4
}
