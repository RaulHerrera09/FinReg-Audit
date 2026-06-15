namespace FinReg.Domain.Events;

public sealed record TransactionFailedEvent(
    Guid TransactionId,
    Guid AccountId,
    string Reason,
    DateTime OccurredOn) : IDomainEvent
{
    public Guid AggregateId => AccountId;
}
