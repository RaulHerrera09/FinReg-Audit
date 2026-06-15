using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using FinReg.Application.Reports.Queries.GetAuditTrail;
using Microsoft.EntityFrameworkCore;

namespace FinReg.Infrastructure.Persistence.Repositories;

public sealed class AuditRepository(AppDbContext db) : IAuditRepository
{
    public async Task<PaginatedList<AuditEventDto>> GetByAccountIdAsync(
        Guid accountId, PaginationParams pagination, CancellationToken ct = default)
    {
        var query = db.AuditEvents.AsNoTracking()
            .Where(e => e.AggregateId == accountId)
            .OrderByDescending(e => e.OccurredOn);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(e => new AuditEventDto
            {
                EventId = e.EventId,
                AggregateId = e.AggregateId,
                AggregateType = e.AggregateType,
                EventType = e.EventType,
                OccurredOn = e.OccurredOn,
                Version = e.Version
            })
            .ToListAsync(ct);

        return new PaginatedList<AuditEventDto>
        {
            Items = items,
            TotalCount = total,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }
}
