using FinReg.Domain.Enums;
using FinReg.Domain.ValueObjects;

namespace FinReg.Domain.Events;

public sealed record TransactionInitiatedEvent(
    Guid TransactionId,
    Guid AccountId,
    Money Amount,
    TransactionType Type,
    string Description,
    DateTime OccurredOn) : IDomainEvent
{
    public Guid AggregateId => AccountId;
}
