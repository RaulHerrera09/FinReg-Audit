using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using FinReg.Domain.Entities;
using FinReg.Domain.Enums;
using FinReg.Domain.Events;
using FinReg.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinReg.Infrastructure.Persistence.EventStore;

public sealed class EventStoreRepository(AppDbContext db) : IEventStore
{
    public async Task AppendEventsAsync(
        Guid aggregateId,
        string aggregateType,
        IEnumerable<IDomainEvent> events,
        int expectedVersion,
        CancellationToken ct = default)
    {
        var eventList = events.ToList();
        var version = expectedVersion;

        var records = eventList.Select(e =>
        {
            version++;
            return new AuditEventRecord
            {
                EventId = Guid.NewGuid(),
                AggregateId = aggregateId,
                AggregateType = aggregateType,
                EventType = e.GetType().Name,
                Payload = EventSerializer.Serialize(e),
                OccurredOn = e.OccurredOn,
                Version = version
            };
        }).ToList();

        await db.AuditEvents.AddRangeAsync(records, ct);
        await ProjectAsync(aggregateId, eventList, ct);
    }

    public async Task<IReadOnlyList<IDomainEvent>> GetEventsAsync(
        Guid aggregateId,
        int fromVersion = 0,
        CancellationToken ct = default)
    {
        var records = await db.AuditEvents
            .Where(e => e.AggregateId == aggregateId && e.Version > fromVersion)
            .OrderBy(e => e.Version)
            .ToListAsync(ct);

        return records
            .Select(r => EventSerializer.Deserialize(r.EventType, r.Payload))
            .ToList();
    }

    public async Task<AccountSnapshot?> GetSnapshotAsync(Guid aggregateId, CancellationToken ct = default)
    {
        var record = await db.AccountSnapshots
            .Where(s => s.AccountId == aggregateId)
            .OrderByDescending(s => s.Version)
            .FirstOrDefaultAsync(ct);

        if (record is null) return null;

        return new AccountSnapshot
        {
            AccountId = record.AccountId,
            StateJson = record.StateJson,
            Version = record.Version,
            CreatedAt = record.CreatedAt
        };
    }

    public async Task SaveSnapshotAsync(AccountSnapshot snapshot, CancellationToken ct = default)
    {
        var record = new AccountSnapshotRecord
        {
            Id = Guid.NewGuid(),
            AccountId = snapshot.AccountId,
            StateJson = snapshot.StateJson,
            Version = snapshot.Version,
            CreatedAt = snapshot.CreatedAt
        };

        await db.AccountSnapshots.AddAsync(record, ct);
    }

    private async Task ProjectAsync(Guid aggregateId, IEnumerable<IDomainEvent> events, CancellationToken ct)
    {
        foreach (var @event in events)
        {
            switch (@event)
            {
                case AccountOpenedEvent e:
                    var account = new AccountReadModel
                    {
                        Id = e.AccountId,
                        AccountNumber = e.AccountNumber.Value,
                        HolderName = e.HolderName,
                        Status = "Active",
                        Balance = e.InitialBalance.Amount,
                        Currency = e.InitialBalance.Currency,
                        RiskLevel = "Low",
                        Version = 1,
                        LastUpdated = e.OccurredOn
                    };
                    await db.Accounts.AddAsync(account, ct);
                    break;

                case AccountSuspendedEvent e:
                    await db.Accounts
                        .Where(a => a.Id == e.AccountId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(a => a.Status, "Suspended")
                            .SetProperty(a => a.LastUpdated, e.OccurredOn), ct);
                    break;

                case AccountClosedEvent e:
                    await db.Accounts
                        .Where(a => a.Id == e.AccountId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(a => a.Status, "Closed")
                            .SetProperty(a => a.LastUpdated, e.OccurredOn), ct);
                    break;

                case AccountReviewInitiatedEvent e:
                    await db.Accounts
                        .Where(a => a.Id == e.AccountId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(a => a.Status, "UnderReview")
                            .SetProperty(a => a.RiskLevel, e.RiskLevel.ToString())
                            .SetProperty(a => a.LastUpdated, e.OccurredOn), ct);
                    break;

                case TransactionInitiatedEvent e:
                    var tx = new TransactionReadModel
                    {
                        Id = e.TransactionId,
                        AccountId = e.AccountId,
                        Amount = e.Amount.Amount,
                        Currency = e.Amount.Currency,
                        Type = e.Type.ToString(),
                        Status = "Pending",
                        Description = e.Description,
                        RiskLevel = RiskLevel.Low.ToString(),
                        InitiatedAt = e.OccurredOn
                    };
                    await db.Transactions.AddAsync(tx, ct);
                    break;

                case TransactionCompletedEvent e:
                    await db.Transactions
                        .Where(t => t.Id == e.TransactionId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(t => t.Status, "Completed")
                            .SetProperty(t => t.CompletedAt, e.OccurredOn), ct);
                    await db.Accounts
                        .Where(a => a.Id == e.AccountId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(a => a.Balance, e.NewBalance.Amount)
                            .SetProperty(a => a.LastUpdated, e.OccurredOn), ct);
                    break;

                case TransactionFailedEvent e:
                    await db.Transactions
                        .Where(t => t.Id == e.TransactionId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(t => t.Status, "Failed")
                            .SetProperty(t => t.FailureReason, e.Reason)
                            .SetProperty(t => t.CompletedAt, e.OccurredOn), ct);
                    break;

                case TransactionFlaggedEvent e:
                    await db.Transactions
                        .Where(t => t.Id == e.TransactionId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(t => t.Status, "Flagged")
                            .SetProperty(t => t.RiskLevel, e.RiskLevel.ToString()), ct);
                    var alert = new AlertRecord
                    {
                        Id = Guid.NewGuid(),
                        TransactionId = e.TransactionId,
                        AccountId = e.AccountId,
                        Severity = e.Severity.ToString(),
                        RiskLevel = e.RiskLevel.ToString(),
                        Reason = e.Reason,
                        OccurredOn = e.OccurredOn
                    };
                    await db.Alerts.AddAsync(alert, ct);
                    break;
            }
        }
    }
}
