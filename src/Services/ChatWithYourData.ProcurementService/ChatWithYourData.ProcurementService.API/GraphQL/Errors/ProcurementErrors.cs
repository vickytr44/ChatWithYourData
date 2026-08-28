namespace ChatWithYourData.ProcurementService.API.GraphQL.Errors;

public sealed class VendorNotFoundException(Guid vendorId)
    : Exception($"Vendor with ID {vendorId} was not found.")
{
    public Guid VendorId { get; } = vendorId;
}

public sealed class VendorCodeAlreadyExistsException(string vendorCode)
    : Exception($"Vendor with code '{vendorCode}' already exists.")
{
    public string VendorCode { get; } = vendorCode;
}

public sealed class PurchaseOrderNotFoundException(Guid poId)
    : Exception($"Purchase order with ID {poId} was not found.")
{
    public Guid PoId { get; } = poId;
}
