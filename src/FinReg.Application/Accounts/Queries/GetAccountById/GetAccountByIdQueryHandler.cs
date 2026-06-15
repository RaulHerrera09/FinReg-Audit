using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using FinReg.Domain.Exceptions;
using MediatR;

namespace FinReg.Application.Accounts.Queries.GetAccountById;

public sealed class GetAccountByIdQueryHandler(IAccountRepository accountRepository)
    : IRequestHandler<GetAccountByIdQuery, ApiResponse<AccountDto>>
{
    public async Task<ApiResponse<AccountDto>> Handle(GetAccountByIdQuery query, CancellationToken ct)
    {
        var account = await accountRepository.GetByIdAsync(query.AccountId, ct)
            ?? throw new AccountNotFoundException(query.AccountId);

        return ApiResponse<AccountDto>.Ok(new AccountDto
        {
            Id = account.Id,
            AccountNumber = account.AccountNumber.Value,
            HolderName = account.HolderName,
            Status = account.Status.ToString(),
            Balance = account.Balance.Amount,
            Currency = account.Balance.Currency,
            RiskLevel = account.RiskLevel.ToString(),
            Version = account.Version
        });
    }
}
