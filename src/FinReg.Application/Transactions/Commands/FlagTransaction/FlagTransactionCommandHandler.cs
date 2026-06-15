using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using FinReg.Domain.Exceptions;
using MediatR;

namespace FinReg.Application.Transactions.Commands.FlagTransaction;

public sealed class FlagTransactionCommandHandler(IAccountRepository accountRepository)
    : IRequestHandler<FlagTransactionCommand, ApiResponse<bool>>
{
    public async Task<ApiResponse<bool>> Handle(FlagTransactionCommand command, CancellationToken ct)
    {
        var account = await accountRepository.GetByIdAsync(command.AccountId, ct)
            ?? throw new AccountNotFoundException(command.AccountId);

        account.FlagTransaction(command.TransactionId, command.RiskLevel, command.Severity, command.Reason);
        await accountRepository.SaveAsync(account, ct);
        return ApiResponse<bool>.Ok(true);
    }
}
