using FinReg.Application.Common.Models;
using MediatR;

namespace FinReg.Application.Alerts.Queries.GetAlerts;

public sealed record GetAlertsQuery(PaginationParams Pagination) : IRequest<ApiResponse<PaginatedList<AlertDto>>>;
