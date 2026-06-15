namespace FinReg.Infrastructure.Persistence.Entities;

public class AuditEventRecord
{
    public Guid EventId { get; set; }
    public Guid AggregateId { get; set; }
    public string AggregateType { get; set; } = null!;
    public string EventType { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public DateTime OccurredOn { get; set; }
    public int Version { get; set; }
}
