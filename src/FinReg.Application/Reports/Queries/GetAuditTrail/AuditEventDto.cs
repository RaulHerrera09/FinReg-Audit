namespace FinReg.Application.Reports.Queries.GetAuditTrail;

public sealed record AuditEventDto
{
    public Guid EventId { get; init; }
    public Guid AggregateId { get; init; }
    public string AggregateType { get; init; } = null!;
    public string EventType { get; init; } = null!;
    public DateTime OccurredOn { get; init; }
    public int Version { get; init; }
}
