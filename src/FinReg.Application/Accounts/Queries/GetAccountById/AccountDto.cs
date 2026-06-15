namespace FinReg.Application.Accounts.Queries.GetAccountById;

public sealed record AccountDto
{
    public Guid Id { get; init; }
    public string AccountNumber { get; init; } = null!;
    public string HolderName { get; init; } = null!;
    public string Status { get; init; } = null!;
    public decimal Balance { get; init; }
    public string Currency { get; init; } = null!;
    public string RiskLevel { get; init; } = null!;
    public int Version { get; init; }
}
