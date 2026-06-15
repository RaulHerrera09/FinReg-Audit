using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using FinReg.Domain.Exceptions;
using FinReg.Domain.ValueObjects;
using MediatR;

namespace FinReg.Application.Transactions.Commands.InitiateTransaction;

public sealed class InitiateTransactionCommandHandler(IAccountRepository accountRepository)
    : IRequestHandler<InitiateTransactionCommand, ApiResponse<Guid>>
{
    public async Task<ApiResponse<Guid>> Handle(InitiateTransactionCommand command, CancellationToken ct)
    {
        var account = await accountRepository.GetByIdAsync(command.AccountId, ct)
            ?? throw new AccountNotFoundException(command.AccountId);

        var amount = new Money(command.Amount, command.Currency);
        var tx = account.InitiateTransaction(amount, command.Type, command.Description);
        await accountRepository.SaveAsync(account, ct);
        return ApiResponse<Guid>.Ok(tx.Id);
    }
}
