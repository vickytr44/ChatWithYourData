using ChatWithYourData.FinanceService.Application.Common.Interfaces;
using ChatWithYourData.FinanceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatWithYourData.FinanceService.Infrastructure.Persistence;

public class FinanceDbContext(DbContextOptions<FinanceDbContext> options) 
    : DbContext(options), IFinanceDbContext
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalLine> JournalLines => Set<JournalLine>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(builder =>
        {
            builder.HasKey(a => a.Id);
            builder.HasIndex(a => a.AccountCode).IsUnique();
            builder.Property(a => a.AccountCode).IsRequired().HasMaxLength(20);
            builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
            builder.Property(a => a.CurrentBalance).HasPrecision(18, 2);
        });

        modelBuilder.Entity<JournalEntry>(builder =>
        {
            builder.HasKey(j => j.Id);
            builder.HasIndex(j => j.EntryNumber).IsUnique();

            modelBuilder.Entity<JournalLine>()
                .HasOne(l => l.JournalEntry)
                .WithMany(j => j.Lines)
                .HasForeignKey(l => l.JournalEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JournalLine>()
                .HasOne(l => l.Account)
                .WithMany(a => a.JournalLines)
                .HasForeignKey(l => l.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<JournalLine>(builder =>
        {
            builder.HasKey(l => l.Id);
            builder.Property(l => l.DebitAmount).HasPrecision(18, 2);
            builder.Property(l => l.CreditAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Invoice>(builder =>
        {
            builder.HasKey(i => i.Id);
            builder.HasIndex(i => i.InvoiceNumber).IsUnique();
            builder.Property(i => i.Subtotal).HasPrecision(18, 2);
            builder.Property(i => i.TaxAmount).HasPrecision(18, 2);
            builder.Property(i => i.TotalAmount).HasPrecision(18, 2);
            builder.Property(i => i.PaidAmount).HasPrecision(18, 2);

            builder.HasMany(i => i.Payments)
                .WithOne(p => p.Invoice)
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payment>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Amount).HasPrecision(18, 2);
        });
    }
}
