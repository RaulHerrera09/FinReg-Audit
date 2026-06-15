namespace FinReg.Domain.Entities;

public sealed class AuditEvent
{
    public Guid EventId { get; private set; }
    public Guid AggregateId { get; private set; }
    public string AggregateType { get; private set; } = null!;
    public string EventType { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public DateTime OccurredOn { get; private set; }
    public int Version { get; private set; }

    private AuditEvent() { }

    public static AuditEvent Create(
        Guid aggregateId,
        string aggregateType,
        string eventType,
        string payload,
        DateTime occurredOn,
        int version) =>
        new()
        {
            EventId = Guid.NewGuid(),
            AggregateId = aggregateId,
            AggregateType = aggregateType,
            EventType = eventType,
            Payload = payload,
            OccurredOn = occurredOn,
            Version = version
        };
}
