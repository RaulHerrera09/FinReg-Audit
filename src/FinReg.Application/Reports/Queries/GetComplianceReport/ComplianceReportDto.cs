namespace FinReg.Application.Reports.Queries.GetComplianceReport;

public sealed record ComplianceReportDto
{
    public Guid AccountId { get; init; }
    public string AccountNumber { get; init; } = null!;
    public string HolderName { get; init; } = null!;
    public string RiskLevel { get; init; } = null!;
    public int TotalTransactions { get; init; }
    public decimal TotalValueGbp { get; init; }
    public int FlaggedTransactions { get; init; }
    public DateTime GeneratedAt { get; init; }
}
