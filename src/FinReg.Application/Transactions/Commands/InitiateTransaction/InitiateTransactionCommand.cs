using FinReg.Application.Common.Models;
using FinReg.Domain.Enums;
using MediatR;

namespace FinReg.Application.Transactions.Commands.InitiateTransaction;

public sealed record InitiateTransactionCommand(
    Guid AccountId,
    decimal Amount,
    string Currency,
    TransactionType Type,
    string Description) : IRequest<ApiResponse<Guid>>;
