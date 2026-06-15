namespace FinReg.Application.Common.Models;

public sealed record AccountSnapshot
{
    public Guid AccountId { get; init; }
    public string StateJson { get; init; } = null!;
    public int Version { get; init; }
    public DateTime CreatedAt { get; init; }
}
