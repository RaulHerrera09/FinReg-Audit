using FinReg.Application.Common.Models;
using MediatR;

namespace FinReg.Application.Accounts.Queries.GetAccountById;

public sealed record GetAccountByIdQuery(Guid AccountId) : IRequest<ApiResponse<AccountDto>>;
