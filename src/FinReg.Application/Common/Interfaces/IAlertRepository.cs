using FinReg.Application.Alerts.Queries.GetAlerts;
using FinReg.Application.Common.Models;

namespace FinReg.Application.Common.Interfaces;

public interface IAlertRepository
{
    Task<PaginatedList<AlertDto>> GetPagedAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<PaginatedList<AlertDto>> GetByAccountIdAsync(Guid accountId, PaginationParams pagination, CancellationToken ct = default);
}
