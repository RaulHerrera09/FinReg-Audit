namespace FinReg.Application.Alerts.Queries.GetAlerts;

public sealed record AlertDto
{
    public Guid TransactionId { get; init; }
    public Guid AccountId { get; init; }
    public string Severity { get; init; } = null!;
    public string RiskLevel { get; init; } = null!;
    public string Reason { get; init; } = null!;
    public DateTime OccurredOn { get; init; }
}
