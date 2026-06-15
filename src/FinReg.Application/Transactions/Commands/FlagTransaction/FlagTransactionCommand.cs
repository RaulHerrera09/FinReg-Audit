using FinReg.Application.Common.Models;
using FinReg.Domain.Enums;
using MediatR;

namespace FinReg.Application.Transactions.Commands.FlagTransaction;

public sealed record FlagTransactionCommand(
    Guid AccountId,
    Guid TransactionId,
    RiskLevel RiskLevel,
    AlertSeverity Severity,
    string Reason) : IRequest<ApiResponse<bool>>;
