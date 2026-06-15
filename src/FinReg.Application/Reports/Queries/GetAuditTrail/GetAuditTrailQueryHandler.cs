using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using MediatR;

namespace FinReg.Application.Reports.Queries.GetAuditTrail;

public sealed class GetAuditTrailQueryHandler(IAuditRepository auditRepository)
    : IRequestHandler<GetAuditTrailQuery, ApiResponse<PaginatedList<AuditEventDto>>>
{
    public async Task<ApiResponse<PaginatedList<AuditEventDto>>> Handle(GetAuditTrailQuery query, CancellationToken ct)
    {
        var page = await auditRepository.GetByAccountIdAsync(query.AccountId, query.Pagination, ct);
        return ApiResponse<PaginatedList<AuditEventDto>>.Ok(page);
    }
}
