using FinReg.Domain.ValueObjects;

namespace FinReg.Domain.Events;

public sealed record AccountOpenedEvent(
    Guid AccountId,
    AccountNumber AccountNumber,
    string HolderName,
    Money InitialBalance,
    DateTime OccurredOn) : IDomainEvent
{
    public Guid AggregateId => AccountId;
}
