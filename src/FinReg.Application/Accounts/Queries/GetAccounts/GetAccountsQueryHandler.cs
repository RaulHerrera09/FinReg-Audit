using FinReg.Application.Accounts.Queries.GetAccountById;
using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using MediatR;

namespace FinReg.Application.Accounts.Queries.GetAccounts;

public sealed class GetAccountsQueryHandler(IAccountRepository accountRepository)
    : IRequestHandler<GetAccountsQuery, ApiResponse<PaginatedList<AccountDto>>>
{
    public async Task<ApiResponse<PaginatedList<AccountDto>>> Handle(GetAccountsQuery query, CancellationToken ct)
    {
        var page = await accountRepository.GetPagedAsync(query.Pagination, ct);
        return ApiResponse<PaginatedList<AccountDto>>.Ok(page);
    }
}
