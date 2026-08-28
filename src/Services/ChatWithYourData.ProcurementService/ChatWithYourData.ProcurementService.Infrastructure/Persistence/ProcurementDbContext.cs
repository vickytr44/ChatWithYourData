using ChatWithYourData.ProcurementService.Application.Common.Interfaces;
using ChatWithYourData.ProcurementService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.ProcurementService.Infrastructure.Persistence;

public class ProcurementDbContext(DbContextOptions<ProcurementDbContext> options) 
    : DbContext(options), IProcurementDbContext
{
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Vendor>(builder =>
        {
            builder.HasKey(v => v.Id);
            builder.HasIndex(v => v.VendorCode).IsUnique();
            builder.HasIndex(v => v.ContactEmail).IsUnique();
            builder.Property(v => v.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<PurchaseOrder>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.HasIndex(p => p.PoNumber).IsUnique();
            builder.Property(p => p.TotalCost).HasPrecision(18, 2);

            builder.HasOne(p => p.Vendor)
                .WithMany(v => v.PurchaseOrders)
                .HasForeignKey(p => p.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.Lines)
                .WithOne(l => l.PurchaseOrder)
                .HasForeignKey(l => l.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.GoodsReceipts)
                .WithOne(g => g.PurchaseOrder)
                .HasForeignKey(g => g.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PurchaseOrderLine>(builder =>
        {
            builder.HasKey(l => l.Id);
            builder.Property(l => l.UnitCost).HasPrecision(18, 2);
            builder.Property(l => l.LineTotal).HasPrecision(18, 2);
        });

        modelBuilder.Entity<GoodsReceipt>(builder =>
        {
            builder.HasKey(g => g.Id);
            builder.HasIndex(g => g.ReceiptNumber).IsUnique();
        });
    }
}
