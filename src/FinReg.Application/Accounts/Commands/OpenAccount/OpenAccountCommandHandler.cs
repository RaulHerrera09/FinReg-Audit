using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using FinReg.Domain.Entities;
using MediatR;

namespace FinReg.Application.Accounts.Commands.OpenAccount;

public sealed class OpenAccountCommandHandler(IAccountRepository accountRepository)
    : IRequestHandler<OpenAccountCommand, ApiResponse<Guid>>
{
    public async Task<ApiResponse<Guid>> Handle(OpenAccountCommand command, CancellationToken ct)
    {
        var account = Account.Open(command.HolderName, command.Currency);
        await accountRepository.SaveAsync(account, ct);
        return ApiResponse<Guid>.Ok(account.Id);
    }
}
