using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using MediatR;

namespace FinReg.Application.Alerts.Queries.GetAlerts;

public sealed class GetAlertsQueryHandler(IAlertRepository alertRepository)
    : IRequestHandler<GetAlertsQuery, ApiResponse<PaginatedList<AlertDto>>>
{
    public async Task<ApiResponse<PaginatedList<AlertDto>>> Handle(GetAlertsQuery query, CancellationToken ct)
    {
        var page = await alertRepository.GetPagedAsync(query.Pagination, ct);
        return ApiResponse<PaginatedList<AlertDto>>.Ok(page);
    }
}
