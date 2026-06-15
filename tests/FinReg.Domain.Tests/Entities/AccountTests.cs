using FinReg.Domain.Entities;
using FinReg.Domain.Enums;
using FinReg.Domain.Exceptions;
using FinReg.Domain.ValueObjects;
using FluentAssertions;

namespace FinReg.Domain.Tests.Entities;

public sealed class AccountTests
{
    [Fact]
    public void Open_ValidArgs_ReturnsActiveAccountWithZeroBalance()
    {
        var account = Account.Open("John Smith", "GBP");

        account.Status.Should().Be(AccountStatus.Active);
        account.HolderName.Should().Be("John Smith");
        account.Balance.Amount.Should().Be(0m);
        account.Balance.Currency.Should().Be("GBP");
        account.RiskLevel.Should().Be(RiskLevel.Low);
        account.DomainEvents.Should().HaveCount(1);
        account.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Open_EmptyHolderName_Throws()
    {
        var act = () => Account.Open("   ", "GBP");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void InitiateTransaction_OnActiveAccount_CreatesPendingTransaction()
    {
        var account = Account.Open("Jane Doe", "GBP");
        account.ClearDomainEvents();

        var amount = new Money(500m, "GBP");
        var tx = account.InitiateTransaction(amount, TransactionType.Credit, "Salary");

        tx.Status.Should().Be(TransactionStatus.Pending);
        tx.Amount.Should().Be(amount);
        tx.Type.Should().Be(TransactionType.Credit);
        account.Transactions.Should().HaveCount(1);
        account.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void CompleteTransaction_Credit_IncreasesBalance()
    {
        var account = Account.Open("Jane Doe", "GBP");
        var tx = account.InitiateTransaction(new Money(1000m, "GBP"), TransactionType.Credit, "Deposit");

        account.CompleteTransaction(tx.Id);

        account.Balance.Amount.Should().Be(1000m);
        account.Transactions.Single().Status.Should().Be(TransactionStatus.Completed);
    }

    [Fact]
    public void CompleteTransaction_Debit_DecreasesBalance()
    {
        var account = Account.Open("Jane Doe", "GBP");
        var credit = account.InitiateTransaction(new Money(1000m, "GBP"), TransactionType.Credit, "Deposit");
        account.CompleteTransaction(credit.Id);

        var debit = account.InitiateTransaction(new Money(300m, "GBP"), TransactionType.Debit, "Withdrawal");
        account.CompleteTransaction(debit.Id);

        account.Balance.Amount.Should().Be(700m);
    }

    [Fact]
    public void CompleteTransaction_DebitExceedsBalance_Throws()
    {
        var account = Account.Open("Jane Doe", "GBP");
        var tx = account.InitiateTransaction(new Money(500m, "GBP"), TransactionType.Debit, "Overdraft attempt");

        var act = () => account.CompleteTransaction(tx.Id);

        act.Should().Throw<InsufficientFundsException>();
    }

    [Fact]
    public void InitiateTransaction_OnSuspendedAccount_Throws()
    {
        var account = Account.Open("Jane Doe", "GBP");
        account.Suspend("Fraud detected");

        var act = () => account.InitiateTransaction(new Money(100m, "GBP"), TransactionType.Credit, "Test");

        act.Should().Throw<AccountSuspendedException>();
    }

    [Fact]
    public void Rehydrate_FromEvents_RestoresCorrectState()
    {
        var original = Account.Open("Test User", "GBP");
        var tx = original.InitiateTransaction(new Money(250m, "GBP"), TransactionType.Credit, "Test deposit");
        original.CompleteTransaction(tx.Id);
        var events = original.DomainEvents.ToList();

        var rehydrated = Account.Rehydrate(events);

        rehydrated.Id.Should().Be(original.Id);
        rehydrated.HolderName.Should().Be("Test User");
        rehydrated.Balance.Amount.Should().Be(250m);
        rehydrated.Status.Should().Be(AccountStatus.Active);
        rehydrated.Version.Should().Be(original.Version);
        rehydrated.Transactions.Should().HaveCount(1);
    }

    [Fact]
    public void Suspend_ActiveAccount_ChangesStatusToSuspended()
    {
        var account = Account.Open("Jane Doe", "GBP");
        account.Suspend("Suspicious activity");
        account.Status.Should().Be(AccountStatus.Suspended);
    }

    [Fact]
    public void Suspend_AlreadySuspended_IsIdempotent()
    {
        var account = Account.Open("Jane Doe", "GBP");
        account.Suspend("First");
        account.ClearDomainEvents();

        account.Suspend("Second");

        account.DomainEvents.Should().BeEmpty();
        account.Status.Should().Be(AccountStatus.Suspended);
    }

    [Fact]
    public void Close_ActiveAccount_ChangesStatusToClosed()
    {
        var account = Account.Open("Jane Doe", "GBP");
        account.Close("Customer request");
        account.Status.Should().Be(AccountStatus.Closed);
    }

    [Fact]
    public void Suspend_ClosedAccount_Throws()
    {
        var account = Account.Open("Jane Doe", "GBP");
        account.Close("Reason");

        var act = () => account.Suspend("Late");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void FlagTransaction_UpdatesRiskLevelAndTransactionStatus()
    {
        var account = Account.Open("Jane Doe", "GBP");
        var tx = account.InitiateTransaction(new Money(50000m, "GBP"), TransactionType.Credit, "Large deposit");
        account.ClearDomainEvents();

        account.FlagTransaction(tx.Id, RiskLevel.High, AlertSeverity.High, "Exceeds FCA threshold");

        account.RiskLevel.Should().Be(RiskLevel.High);
        account.Transactions.Single().Status.Should().Be(TransactionStatus.Flagged);
        account.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void FailTransaction_PendingTransaction_ChangesStatusToFailed()
    {
        var account = Account.Open("Jane Doe", "GBP");
        var tx = account.InitiateTransaction(new Money(100m, "GBP"), TransactionType.Debit, "ATM");

        account.FailTransaction(tx.Id, "Card declined");

        account.Transactions.Single().Status.Should().Be(TransactionStatus.Failed);
        account.Transactions.Single().FailureReason.Should().Be("Card declined");
    }

    [Fact]
    public void InitiateReview_ClosedAccount_Throws()
    {
        var account = Account.Open("Jane Doe", "GBP");
        account.Close("Reason");

        var act = () => account.InitiateReview(RiskLevel.High, "Suspicious");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void InitiateReview_ActiveAccount_SetsUnderReviewStatus()
    {
        var account = Account.Open("Jane Doe", "GBP");

        account.InitiateReview(RiskLevel.Medium, "Routine check");

        account.Status.Should().Be(AccountStatus.UnderReview);
        account.RiskLevel.Should().Be(RiskLevel.Medium);
    }
}
