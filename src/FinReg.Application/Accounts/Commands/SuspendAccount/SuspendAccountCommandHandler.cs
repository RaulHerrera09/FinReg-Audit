using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using FinReg.Domain.Exceptions;
using MediatR;

namespace FinReg.Application.Accounts.Commands.SuspendAccount;

public sealed class SuspendAccountCommandHandler(IAccountRepository accountRepository)
    : IRequestHandler<SuspendAccountCommand, ApiResponse<bool>>
{
    public async Task<ApiResponse<bool>> Handle(SuspendAccountCommand command, CancellationToken ct)
    {
        var account = await accountRepository.GetByIdAsync(command.AccountId, ct)
            ?? throw new AccountNotFoundException(command.AccountId);

        account.Suspend(command.Reason);
        await accountRepository.SaveAsync(account, ct);
        return ApiResponse<bool>.Ok(true);
    }
}
