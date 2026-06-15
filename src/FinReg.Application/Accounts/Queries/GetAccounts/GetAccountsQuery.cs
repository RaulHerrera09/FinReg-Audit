using FinReg.Application.Accounts.Queries.GetAccountById;
using FinReg.Application.Common.Models;
using MediatR;

namespace FinReg.Application.Accounts.Queries.GetAccounts;

public sealed record GetAccountsQuery(PaginationParams Pagination) : IRequest<ApiResponse<PaginatedList<AccountDto>>>;
