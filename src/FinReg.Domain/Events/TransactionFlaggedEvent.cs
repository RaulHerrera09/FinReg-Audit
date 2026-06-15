using FinReg.Domain.Enums;

namespace FinReg.Domain.Events;

public sealed record TransactionFlaggedEvent(
    Guid TransactionId,
    Guid AccountId,
    RiskLevel RiskLevel,
    AlertSeverity Severity,
    string Reason,
    DateTime OccurredOn) : IDomainEvent
{
    public Guid AggregateId => AccountId;
}
