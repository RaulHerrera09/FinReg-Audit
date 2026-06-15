using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using FinReg.Domain.Exceptions;
using MediatR;

namespace FinReg.Application.Transactions.Commands.CompleteTransaction;

public sealed class CompleteTransactionCommandHandler(
    IAccountRepository accountRepository,
    IRiskAssessmentService riskAssessmentService)
    : IRequestHandler<CompleteTransactionCommand, ApiResponse<bool>>
{
    public async Task<ApiResponse<bool>> Handle(CompleteTransactionCommand command, CancellationToken ct)
    {
        var account = await accountRepository.GetByIdAsync(command.AccountId, ct)
            ?? throw new AccountNotFoundException(command.AccountId);

        account.CompleteTransaction(command.TransactionId);

        var tx = account.Transactions.First(t => t.Id == command.TransactionId);
        var risk = riskAssessmentService.Assess(tx.Amount.Amount, tx.Amount.Currency);
        if (risk.ShouldFlag)
            account.FlagTransaction(command.TransactionId, risk.RiskLevel, risk.Severity, risk.Reason);

        await accountRepository.SaveAsync(account, ct);
        return ApiResponse<bool>.Ok(true);
    }
}
