using ChatWithYourData.ProcurementService.Application.Common;
using ChatWithYourData.ProcurementService.Application.Common.Interfaces;
using ChatWithYourData.ProcurementService.Application.Features.Procurement.DTOs;
using ChatWithYourData.ProcurementService.Domain.Entities;
using ChatWithYourData.ProcurementService.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.ProcurementService.Application.Features.Procurement.Commands;

public record CreateVendorCommand(
    string Name,
    string ContactEmail,
    string Phone,
    string Address,
    int PaymentTermsDays,
    string TaxId
) : IRequest<Result<VendorDto>>;

public class CreateVendorCommandValidator : AbstractValidator<CreateVendorCommand>
{
    public CreateVendorCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.PaymentTermsDays).GreaterThanOrEqualTo(0);
    }
}

public class CreateVendorCommandHandler(IProcurementDbContext dbContext)
    : IRequestHandler<CreateVendorCommand, Result<VendorDto>>
{
    public async Task<Result<VendorDto>> Handle(CreateVendorCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await dbContext.Vendors.AnyAsync(v => v.ContactEmail == request.ContactEmail, cancellationToken);
        if (emailExists)
            return Result<VendorDto>.Failure($"Vendor with email '{request.ContactEmail}' already exists.");

        var count = await dbContext.Vendors.CountAsync(cancellationToken);
        var vendor = new Vendor
        {
            VendorCode = $"VND-{(count + 1):D5}",
            Name = request.Name,
            ContactEmail = request.ContactEmail,
            Phone = request.Phone,
            Address = request.Address,
            PaymentTermsDays = request.PaymentTermsDays,
            TaxId = request.TaxId,
            IsActive = true
        };

        dbContext.Vendors.Add(vendor);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<VendorDto>.Success(new VendorDto(
            vendor.Id,
            vendor.VendorCode,
            vendor.Name,
            vendor.ContactEmail,
            vendor.Phone,
            vendor.Address,
            vendor.PaymentTermsDays,
            vendor.TaxId,
            vendor.IsActive,
            vendor.CreatedAtUtc
        ));
    }
}

public record CreatePurchaseOrderCommand(
    Guid VendorId,
    DateTime? ExpectedDeliveryDateUtc,
    string Notes,
    List<CreatePoLineInput> Lines
) : IRequest<Result<PurchaseOrderDto>>;

public class CreatePurchaseOrderCommandHandler(IProcurementDbContext dbContext)
    : IRequestHandler<CreatePurchaseOrderCommand, Result<PurchaseOrderDto>>
{
    public async Task<Result<PurchaseOrderDto>> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var vendor = await dbContext.Vendors.FindAsync([request.VendorId], cancellationToken);
        if (vendor == null)
            return Result<PurchaseOrderDto>.Failure($"Vendor with ID {request.VendorId} not found.");

        if (request.Lines == null || request.Lines.Count == 0)
            return Result<PurchaseOrderDto>.Failure("Purchase order must contain at least one line item.");

        var count = await dbContext.PurchaseOrders.CountAsync(cancellationToken);
        var po = new PurchaseOrder
        {
            PoNumber = $"PO-{(count + 1):D5}",
            VendorId = request.VendorId,
            OrderDateUtc = DateTime.UtcNow,
            ExpectedDeliveryDateUtc = request.ExpectedDeliveryDateUtc ?? DateTime.UtcNow.AddDays(14),
            Status = PurchaseOrderStatus.Submitted,
            Notes = request.Notes
        };

        decimal total = 0;
        foreach (var lineInput in request.Lines)
        {
            var lineTotal = lineInput.QuantityOrdered * lineInput.UnitCost;
            total += lineTotal;

            po.Lines.Add(new PurchaseOrderLine
            {
                ProductId = lineInput.ProductId,
                Sku = lineInput.Sku,
                ProductName = lineInput.ProductName,
                QuantityOrdered = lineInput.QuantityOrdered,
                QuantityReceived = 0,
                UnitCost = lineInput.UnitCost,
                LineTotal = lineTotal
            });
        }
        po.TotalCost = total;

        dbContext.PurchaseOrders.Add(po);
        await dbContext.SaveChangesAsync(cancellationToken);

        var lineDtos = po.Lines.Select(l => new PurchaseOrderLineDto(
            l.Id,
            l.PurchaseOrderId,
            l.ProductId,
            l.Sku,
            l.ProductName,
            l.QuantityOrdered,
            l.QuantityReceived,
            l.UnitCost,
            l.LineTotal
        )).ToList();

        return Result<PurchaseOrderDto>.Success(new PurchaseOrderDto(
            po.Id,
            po.PoNumber,
            po.VendorId,
            po.OrderDateUtc,
            po.ExpectedDeliveryDateUtc,
            po.Status,
            po.TotalCost,
            po.Notes,
            lineDtos
        ));
    }
}
