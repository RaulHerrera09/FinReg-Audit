namespace FinReg.Domain.Events;

public sealed record AccountSuspendedEvent(
    Guid AccountId,
    string Reason,
    DateTime OccurredOn) : IDomainEvent
{
    public Guid AggregateId => AccountId;
}
