using FinReg.Application.Common.Models;
using FinReg.Domain.Events;

namespace FinReg.Application.Common.Interfaces;

public interface IEventStore
{
    Task AppendEventsAsync(Guid aggregateId, string aggregateType, IEnumerable<IDomainEvent> events, int expectedVersion, CancellationToken ct = default);
    Task<IReadOnlyList<IDomainEvent>> GetEventsAsync(Guid aggregateId, int fromVersion = 0, CancellationToken ct = default);
    Task<AccountSnapshot?> GetSnapshotAsync(Guid aggregateId, CancellationToken ct = default);
    Task SaveSnapshotAsync(AccountSnapshot snapshot, CancellationToken ct = default);
}
