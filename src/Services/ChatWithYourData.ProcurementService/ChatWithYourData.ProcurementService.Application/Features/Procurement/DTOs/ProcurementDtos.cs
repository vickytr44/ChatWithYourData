using ChatWithYourData.ProcurementService.Domain.Enums;

namespace ChatWithYourData.ProcurementService.Application.Features.Procurement.DTOs;

public record VendorDto(
    Guid Id,
    string VendorCode,
    string Name,
    string ContactEmail,
    string Phone,
    string Address,
    int PaymentTermsDays,
    string TaxId,
    bool IsActive,
    DateTime CreatedAtUtc
);

public record PurchaseOrderLineDto(
    Guid Id,
    Guid PurchaseOrderId,
    Guid ProductId,
    string Sku,
    string ProductName,
    int QuantityOrdered,
    int QuantityReceived,
    decimal UnitCost,
    decimal LineTotal
);

public record PurchaseOrderDto(
    Guid Id,
    string PoNumber,
    Guid VendorId,
    DateTime OrderDateUtc,
    DateTime? ExpectedDeliveryDateUtc,
    PurchaseOrderStatus Status,
    decimal TotalCost,
    string Notes,
    IReadOnlyList<PurchaseOrderLineDto> Lines
);

public record CreatePoLineInput(
    Guid ProductId,
    string Sku,
    string ProductName,
    int QuantityOrdered,
    decimal UnitCost
);
