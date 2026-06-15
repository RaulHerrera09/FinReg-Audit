using FinReg.Domain.Enums;

namespace FinReg.Domain.Events;

public sealed record AccountReviewInitiatedEvent(
    Guid AccountId,
    RiskLevel RiskLevel,
    string Reason,
    DateTime OccurredOn) : IDomainEvent
{
    public Guid AggregateId => AccountId;
}
