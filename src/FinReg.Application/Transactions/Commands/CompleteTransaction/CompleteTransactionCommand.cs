using FinReg.Application.Common.Models;
using MediatR;

namespace FinReg.Application.Transactions.Commands.CompleteTransaction;

public sealed record CompleteTransactionCommand(Guid AccountId, Guid TransactionId) : IRequest<ApiResponse<bool>>;
