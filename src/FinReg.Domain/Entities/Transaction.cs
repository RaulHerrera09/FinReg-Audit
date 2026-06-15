using FinReg.Domain.Enums;
using FinReg.Domain.ValueObjects;

namespace FinReg.Domain.Entities;

public sealed class Transaction
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public TransactionType Type { get; private set; }
    public TransactionStatus Status { get; private set; }
    public string Description { get; private set; } = null!;
    public RiskLevel RiskLevel { get; private set; }
    public DateTime InitiatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? FailureReason { get; private set; }

    private Transaction() { }

    internal static Transaction Create(
        Guid id,
        Guid accountId,
        Money amount,
        TransactionType type,
        string description,
        DateTime initiatedAt) =>
        new()
        {
            Id = id,
            AccountId = accountId,
            Amount = amount,
            Type = type,
            Status = TransactionStatus.Pending,
            Description = description,
            RiskLevel = RiskLevel.Low,
            InitiatedAt = initiatedAt
        };

    internal void MarkCompleted(DateTime completedAt)
    {
        Status = TransactionStatus.Completed;
        CompletedAt = completedAt;
    }

    internal void MarkFailed(string reason)
    {
        Status = TransactionStatus.Failed;
        FailureReason = reason;
    }

    internal void MarkFlagged(RiskLevel riskLevel)
    {
        Status = TransactionStatus.Flagged;
        RiskLevel = riskLevel;
    }
}
