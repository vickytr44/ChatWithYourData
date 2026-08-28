using ChatWithYourData.SalesService.Application.Common.Interfaces;
using ChatWithYourData.SalesService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatWithYourData.SalesService.Infrastructure.Persistence;

public class SalesDbContext(DbContextOptions<SalesDbContext> options) 
    : DbContext(options), ISalesDbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
    public DbSet<Shipment> Shipments => Set<Shipment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(builder =>
        {
            builder.HasKey(c => c.Id);
            builder.HasIndex(c => c.CustomerNumber).IsUnique();
            builder.HasIndex(c => c.Email).IsUnique();
            builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
            builder.Property(c => c.CreditLimit).HasPrecision(18, 2);
        });

        modelBuilder.Entity<SalesOrder>(builder =>
        {
            builder.HasKey(o => o.Id);
            builder.HasIndex(o => o.OrderNumber).IsUnique();
            builder.Property(o => o.TotalAmount).HasPrecision(18, 2);

            builder.HasOne(o => o.Customer)
                .WithMany(c => c.SalesOrders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(o => o.Lines)
                .WithOne(l => l.SalesOrder)
                .HasForeignKey(l => l.SalesOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(o => o.Shipment)
                .WithOne(s => s.SalesOrder)
                .HasForeignKey<Shipment>(s => s.SalesOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SalesOrderLine>(builder =>
        {
            builder.HasKey(l => l.Id);
            builder.Property(l => l.UnitPrice).HasPrecision(18, 2);
            builder.Property(l => l.DiscountPercentage).HasPrecision(5, 2);
            builder.Property(l => l.LineTotal).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Shipment>(builder =>
        {
            builder.HasKey(s => s.Id);
            builder.HasIndex(s => s.TrackingNumber).IsUnique();
        });
    }
}
