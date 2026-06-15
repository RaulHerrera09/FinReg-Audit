using FinReg.Domain.Enums;
using FinReg.Domain.Events;
using FinReg.Domain.Exceptions;
using FinReg.Domain.ValueObjects;

namespace FinReg.Domain.Entities;

public sealed class Account
{
    private readonly List<IDomainEvent> _domainEvents = new();
    private readonly List<Transaction> _transactions = new();

    public Guid Id { get; private set; }
    public AccountNumber AccountNumber { get; private set; } = null!;
    public string HolderName { get; private set; } = null!;
    public AccountStatus Status { get; private set; }
    public Money Balance { get; private set; } = null!;
    public RiskLevel RiskLevel { get; private set; }
    public int Version { get; private set; }

    public IReadOnlyList<Transaction> Transactions => _transactions.AsReadOnly();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Account() { }

    public static Account Rehydrate(IEnumerable<IDomainEvent> events)
    {
        var account = new Account();
        foreach (var @event in events)
            account.Apply(@event);
        return account;
    }

    public static Account Open(string holderName, string currency)
    {
        if (string.IsNullOrWhiteSpace(holderName))
            throw new ArgumentException("Holder name is required.", nameof(holderName));

        var account = new Account();
        account.Raise(new AccountOpenedEvent(
            AccountId: Guid.NewGuid(),
            AccountNumber: AccountNumber.Generate(),
            HolderName: holderName.Trim(),
            InitialBalance: new Money(0m, currency),
            OccurredOn: DateTime.UtcNow));
        return account;
    }

    public Transaction InitiateTransaction(Money amount, TransactionType type, string description)
    {
        EnsureActive();
        var transactionId = Guid.NewGuid();
        Raise(new TransactionInitiatedEvent(
            TransactionId: transactionId,
            AccountId: Id,
            Amount: amount,
            Type: type,
            Description: description,
            OccurredOn: DateTime.UtcNow));
        return _transactions.First(t => t.Id == transactionId);
    }

    public void CompleteTransaction(Guid transactionId)
    {
        var tx = GetTransaction(transactionId);
        if (tx.Status != TransactionStatus.Pending)
            throw new InvalidTransactionException(transactionId, $"Expected Pending, got {tx.Status}.");

        Money newBalance;
        if (tx.Type is TransactionType.Debit or TransactionType.Transfer)
        {
            if (Balance.Amount < tx.Amount.Amount)
                throw new InsufficientFundsException(Id, tx.Amount, Balance);
            newBalance = Balance.Subtract(tx.Amount);
        }
        else
        {
            newBalance = Balance.Add(tx.Amount);
        }

        Raise(new TransactionCompletedEvent(
            TransactionId: transactionId,
            AccountId: Id,
            NewBalance: newBalance,
            OccurredOn: DateTime.UtcNow));
    }

    public void FailTransaction(Guid transactionId, string reason)
    {
        var tx = GetTransaction(transactionId);
        if (tx.Status != TransactionStatus.Pending)
            throw new InvalidTransactionException(transactionId, $"Expected Pending, got {tx.Status}.");

        Raise(new TransactionFailedEvent(
            TransactionId: transactionId,
            AccountId: Id,
            Reason: reason,
            OccurredOn: DateTime.UtcNow));
    }

    public void FlagTransaction(Guid transactionId, RiskLevel riskLevel, AlertSeverity severity, string reason)
    {
        GetTransaction(transactionId);
        Raise(new TransactionFlaggedEvent(
            TransactionId: transactionId,
            AccountId: Id,
            RiskLevel: riskLevel,
            Severity: severity,
            Reason: reason,
            OccurredOn: DateTime.UtcNow));
    }

    public void Suspend(string reason)
    {
        if (Status == AccountStatus.Closed)
            throw new DomainException($"Account {Id} is closed and cannot be suspended.");
        if (Status == AccountStatus.Suspended)
            return;
        Raise(new AccountSuspendedEvent(AccountId: Id, Reason: reason, OccurredOn: DateTime.UtcNow));
    }

    public void InitiateReview(RiskLevel riskLevel, string reason)
    {
        if (Status == AccountStatus.Closed)
            throw new DomainException($"Account {Id} is closed.");
        Raise(new AccountReviewInitiatedEvent(AccountId: Id, RiskLevel: riskLevel, Reason: reason, OccurredOn: DateTime.UtcNow));
    }

    public void Close(string reason)
    {
        if (Status == AccountStatus.Closed)
            throw new DomainException($"Account {Id} is already closed.");
        Raise(new AccountClosedEvent(AccountId: Id, Reason: reason, OccurredOn: DateTime.UtcNow));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void Raise(IDomainEvent @event)
    {
        Apply(@event);
        _domainEvents.Add(@event);
    }

    private void Apply(IDomainEvent @event)
    {
        Version++;
        switch (@event)
        {
            case AccountOpenedEvent e:          Apply(e); break;
            case AccountSuspendedEvent e:       Apply(e); break;
            case AccountClosedEvent e:          Apply(e); break;
            case AccountReviewInitiatedEvent e: Apply(e); break;
            case TransactionInitiatedEvent e:   Apply(e); break;
            case TransactionCompletedEvent e:   Apply(e); break;
            case TransactionFailedEvent e:      Apply(e); break;
            case TransactionFlaggedEvent e:     Apply(e); break;
        }
    }

    private void Apply(AccountOpenedEvent e)
    {
        Id = e.AccountId;
        AccountNumber = e.AccountNumber;
        HolderName = e.HolderName;
        Balance = e.InitialBalance;
        Status = AccountStatus.Active;
        RiskLevel = RiskLevel.Low;
    }

    private void Apply(AccountSuspendedEvent _) => Status = AccountStatus.Suspended;
    private void Apply(AccountClosedEvent _)    => Status = AccountStatus.Closed;

    private void Apply(AccountReviewInitiatedEvent e)
    {
        Status = AccountStatus.UnderReview;
        RiskLevel = e.RiskLevel;
    }

    private void Apply(TransactionInitiatedEvent e) =>
        _transactions.Add(Transaction.Create(e.TransactionId, e.AccountId, e.Amount, e.Type, e.Description, e.OccurredOn));

    private void Apply(TransactionCompletedEvent e)
    {
        Balance = e.NewBalance;
        GetTransaction(e.TransactionId).MarkCompleted(e.OccurredOn);
    }

    private void Apply(TransactionFailedEvent e) =>
        GetTransaction(e.TransactionId).MarkFailed(e.Reason);

    private void Apply(TransactionFlaggedEvent e)
    {
        RiskLevel = e.RiskLevel;
        GetTransaction(e.TransactionId).MarkFlagged(e.RiskLevel);
    }

    private void EnsureActive()
    {
        if (Status != AccountStatus.Active)
            throw new AccountSuspendedException(Id, Status);
    }

    private Transaction GetTransaction(Guid transactionId) =>
        _transactions.FirstOrDefault(t => t.Id == transactionId)
        ?? throw new InvalidTransactionException(transactionId);
}
