using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using MediatR;

namespace FinReg.Application.Transactions.Queries.GetTransactionsByAccount;

public sealed class GetTransactionsByAccountQueryHandler(ITransactionRepository transactionRepository)
    : IRequestHandler<GetTransactionsByAccountQuery, ApiResponse<PaginatedList<TransactionDto>>>
{
    public async Task<ApiResponse<PaginatedList<TransactionDto>>> Handle(GetTransactionsByAccountQuery query, CancellationToken ct)
    {
        var page = await transactionRepository.GetByAccountIdAsync(query.AccountId, query.Pagination, ct);
        return ApiResponse<PaginatedList<TransactionDto>>.Ok(page);
    }
}
