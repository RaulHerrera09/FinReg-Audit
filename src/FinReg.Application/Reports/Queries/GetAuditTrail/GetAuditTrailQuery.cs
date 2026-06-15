using FinReg.Application.Common.Models;
using MediatR;

namespace FinReg.Application.Reports.Queries.GetAuditTrail;

public sealed record GetAuditTrailQuery(Guid AccountId, PaginationParams Pagination)
    : IRequest<ApiResponse<PaginatedList<AuditEventDto>>>;
