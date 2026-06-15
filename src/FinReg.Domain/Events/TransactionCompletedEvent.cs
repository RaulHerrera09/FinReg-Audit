using FinReg.Domain.ValueObjects;

namespace FinReg.Domain.Events;

public sealed record TransactionCompletedEvent(
    Guid TransactionId,
    Guid AccountId,
    Money NewBalance,
    DateTime OccurredOn) : IDomainEvent
{
    public Guid AggregateId => AccountId;
}
