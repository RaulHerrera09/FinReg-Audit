using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using FinReg.Domain.Exceptions;
using MediatR;

namespace FinReg.Application.Transactions.Commands.CompleteTransaction;

public sealed class CompleteTransactionCommandHandler(IAccountRepository accountRepository)
    : IRequestHandler<CompleteTransactionCommand, ApiResponse<bool>>
{
    public async Task<ApiResponse<bool>> Handle(CompleteTransactionCommand command, CancellationToken ct)
    {
        var account = await accountRepository.GetByIdAsync(command.AccountId, ct)
            ?? throw new AccountNotFoundException(command.AccountId);

        account.CompleteTransaction(command.TransactionId);
        await accountRepository.SaveAsync(account, ct);
        return ApiResponse<bool>.Ok(true);
    }
}
