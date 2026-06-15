namespace FinReg.Application.Transactions.Queries.GetTransactionsByAccount;

public sealed record TransactionDto
{
    public Guid Id { get; init; }
    public Guid AccountId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = null!;
    public string Type { get; init; } = null!;
    public string Status { get; init; } = null!;
    public string Description { get; init; } = null!;
    public string RiskLevel { get; init; } = null!;
    public DateTime InitiatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string? FailureReason { get; init; }
}
