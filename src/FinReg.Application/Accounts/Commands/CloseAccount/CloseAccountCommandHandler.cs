using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using FinReg.Domain.Exceptions;
using MediatR;

namespace FinReg.Application.Accounts.Commands.CloseAccount;

public sealed class CloseAccountCommandHandler(IAccountRepository accountRepository)
    : IRequestHandler<CloseAccountCommand, ApiResponse<bool>>
{
    public async Task<ApiResponse<bool>> Handle(CloseAccountCommand command, CancellationToken ct)
    {
        var account = await accountRepository.GetByIdAsync(command.AccountId, ct)
            ?? throw new AccountNotFoundException(command.AccountId);

        account.Close(command.Reason);
        await accountRepository.SaveAsync(account, ct);
        return ApiResponse<bool>.Ok(true);
    }
}
