using ChatWithYourData.FinanceService.Application.Common.Interfaces;
using ChatWithYourData.FinanceService.Application.Features.Finance.Commands;
using ChatWithYourData.FinanceService.Application.Features.Finance.DTOs;
using ChatWithYourData.FinanceService.Domain.Entities;
using ChatWithYourData.FinanceService.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ChatWithYourData.FinanceService.UnitTests;

public class CreateAccountCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenAccountCodeAlreadyExists_ReturnsFailureResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MockFinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new MockFinanceDbContext(options);
        dbContext.Accounts.Add(new Account
        {
            AccountCode = "1010",
            Name = "Cash",
            Type = AccountType.Asset
        });
        await dbContext.SaveChangesAsync();

        var handler = new CreateAccountCommandHandler(dbContext);
        var command = new CreateAccountCommand("1010", "Duplicate Cash", AccountType.Asset, "Desc", 0);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ReturnsSuccessWithAccountDto()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MockFinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new MockFinanceDbContext(options);
        var handler = new CreateAccountCommandHandler(dbContext);
        var command = new CreateAccountCommand("5020", "Cloud Hosting Expense", AccountType.Expense, "AWS / Azure costs", 5000);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.AccountCode.Should().Be("5020");
        result.Value.Name.Should().Be("Cloud Hosting Expense");
        result.Value.Type.Should().Be(AccountType.Expense);
        result.Value.CurrentBalance.Should().Be(5000);
    }
}

public class PostJournalEntryCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenDebitsAndCreditsDoNotBalance_ReturnsFailureResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MockFinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new MockFinanceDbContext(options);
        var handler = new PostJournalEntryCommandHandler(dbContext);
        var command = new PostJournalEntryCommand("Unbalanced Entry", "REF-001", new List<CreateJournalLineInput>
        {
            new(Guid.NewGuid(), 1000m, 0m, "Debit line"),
            new(Guid.NewGuid(), 0m, 800m, "Credit line (unbalanced)")
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("must equal total credits");
    }

    [Fact]
    public async Task Handle_WhenBalancedEntry_UpdatesAccountBalancesAndCreatesJournal()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MockFinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new MockFinanceDbContext(options);
        var cashAccount = new Account { Id = Guid.NewGuid(), AccountCode = "1010", Name = "Cash", Type = AccountType.Asset, CurrentBalance = 10000m };
        var revenueAccount = new Account { Id = Guid.NewGuid(), AccountCode = "4010", Name = "Revenue", Type = AccountType.Revenue, CurrentBalance = 50000m };
        dbContext.Accounts.AddRange(cashAccount, revenueAccount);
        await dbContext.SaveChangesAsync();

        var handler = new PostJournalEntryCommandHandler(dbContext);
        var command = new PostJournalEntryCommand("Customer Cash Sale", "INV-100", new List<CreateJournalLineInput>
        {
            new(cashAccount.Id, 2500m, 0m, "Cash received (debit asset)"),
            new(revenueAccount.Id, 0m, 2500m, "Sales revenue (credit revenue)")
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsPosted.Should().BeTrue();
        result.Value.Lines.Should().HaveCount(2);

        var updatedCash = await dbContext.Accounts.FindAsync([cashAccount.Id]);
        var updatedRevenue = await dbContext.Accounts.FindAsync([revenueAccount.Id]);

        updatedCash!.CurrentBalance.Should().Be(12500m); // 10000 + 2500
        updatedRevenue!.CurrentBalance.Should().Be(52500m); // 50000 + 2500
    }
}

public class MockFinanceDbContext(DbContextOptions<MockFinanceDbContext> options) : DbContext(options), IFinanceDbContext
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalLine> JournalLines => Set<JournalLine>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payment> Payments => Set<Payment>();
}
