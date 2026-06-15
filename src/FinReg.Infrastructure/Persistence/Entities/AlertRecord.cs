namespace FinReg.Infrastructure.Persistence.Entities;

public class AlertRecord
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public Guid AccountId { get; set; }
    public string Severity { get; set; } = null!;
    public string RiskLevel { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public DateTime OccurredOn { get; set; }
}
