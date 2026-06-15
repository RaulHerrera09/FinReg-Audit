using FinReg.Application.Common.Models;
using FinReg.Application.Transactions.Queries.GetTransactionsByAccount;
using MediatR;

namespace FinReg.Application.Transactions.Queries.GetTransactionById;

public sealed record GetTransactionByIdQuery(Guid TransactionId) : IRequest<ApiResponse<TransactionDto>>;
