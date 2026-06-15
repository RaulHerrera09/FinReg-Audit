using FinReg.Application.Alerts.Queries.GetAlerts;
using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using MediatR;

namespace FinReg.Application.Alerts.Queries.GetAlertsByAccount;

public sealed class GetAlertsByAccountQueryHandler(IAlertRepository alertRepository)
    : IRequestHandler<GetAlertsByAccountQuery, ApiResponse<PaginatedList<AlertDto>>>
{
    public async Task<ApiResponse<PaginatedList<AlertDto>>> Handle(
        GetAlertsByAccountQuery query, CancellationToken ct)
    {
        var page = await alertRepository.GetByAccountIdAsync(query.AccountId, query.Pagination, ct);
        return ApiResponse<PaginatedList<AlertDto>>.Ok(page);
    }
}
