using FinReg.Application.Common.Models;
using FinReg.Application.Reports.Queries.GetAuditTrail;

namespace FinReg.Application.Common.Interfaces;

public interface IAuditRepository
{
    Task<PaginatedList<AuditEventDto>> GetByAccountIdAsync(Guid accountId, PaginationParams pagination, CancellationToken ct = default);
}
