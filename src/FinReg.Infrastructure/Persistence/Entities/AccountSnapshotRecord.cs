namespace FinReg.Infrastructure.Persistence.Entities;

public class AccountSnapshotRecord
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string StateJson { get; set; } = null!;
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
}
