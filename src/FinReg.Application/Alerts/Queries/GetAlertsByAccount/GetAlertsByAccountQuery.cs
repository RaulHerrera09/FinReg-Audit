using FinReg.Application.Alerts.Queries.GetAlerts;
using FinReg.Application.Common.Models;
using MediatR;

namespace FinReg.Application.Alerts.Queries.GetAlertsByAccount;

public sealed record GetAlertsByAccountQuery(Guid AccountId, PaginationParams Pagination)
    : IRequest<ApiResponse<PaginatedList<AlertDto>>>;
