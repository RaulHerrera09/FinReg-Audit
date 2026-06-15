using FinReg.Application.Common.Models;
using MediatR;

namespace FinReg.Application.Transactions.Queries.GetTransactionsByAccount;

public sealed record GetTransactionsByAccountQuery(Guid AccountId, PaginationParams Pagination)
    : IRequest<ApiResponse<PaginatedList<TransactionDto>>>;
