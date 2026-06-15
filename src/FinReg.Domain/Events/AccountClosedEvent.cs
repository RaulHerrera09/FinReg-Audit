namespace FinReg.Domain.Events;

public sealed record AccountClosedEvent(
    Guid AccountId,
    string Reason,
    DateTime OccurredOn) : IDomainEvent
{
    public Guid AggregateId => AccountId;
}
