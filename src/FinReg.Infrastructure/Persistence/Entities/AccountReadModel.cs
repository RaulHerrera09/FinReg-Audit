namespace FinReg.Infrastructure.Persistence.Entities;

public class AccountReadModel
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = null!;
    public string HolderName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = null!;
    public string RiskLevel { get; set; } = null!;
    public int Version { get; set; }
    public DateTime LastUpdated { get; set; }
}
