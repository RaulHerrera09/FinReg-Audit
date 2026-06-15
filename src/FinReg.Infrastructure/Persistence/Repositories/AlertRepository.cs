using FinReg.Application.Alerts.Queries.GetAlerts;
using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace FinReg.Infrastructure.Persistence.Repositories;

public sealed class AlertRepository(AppDbContext db) : IAlertRepository
{
    public async Task<PaginatedList<AlertDto>> GetPagedAsync(PaginationParams pagination, CancellationToken ct = default)
    {
        var query = db.Alerts.AsNoTracking().OrderByDescending(a => a.OccurredOn);
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(a => new AlertDto
            {
                TransactionId = a.TransactionId,
                AccountId = a.AccountId,
                Severity = a.Severity,
                RiskLevel = a.RiskLevel,
                Reason = a.Reason,
                OccurredOn = a.OccurredOn
            })
            .ToListAsync(ct);

        return new PaginatedList<AlertDto>
        {
            Items = items,
            TotalCount = total,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<PaginatedList<AlertDto>> GetByAccountIdAsync(
        Guid accountId, PaginationParams pagination, CancellationToken ct = default)
    {
        var query = db.Alerts.AsNoTracking()
            .Where(a => a.AccountId == accountId)
            .OrderByDescending(a => a.OccurredOn);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(a => new AlertDto
            {
                TransactionId = a.TransactionId,
                AccountId = a.AccountId,
                Severity = a.Severity,
                RiskLevel = a.RiskLevel,
                Reason = a.Reason,
                OccurredOn = a.OccurredOn
            })
            .ToListAsync(ct);

        return new PaginatedList<AlertDto>
        {
            Items = items,
            TotalCount = total,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }
}
