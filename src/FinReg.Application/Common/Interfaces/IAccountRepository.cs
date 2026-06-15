using FinReg.Application.Accounts.Queries.GetAccountById;
using FinReg.Application.Common.Models;
using FinReg.Domain.Entities;

namespace FinReg.Application.Common.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid accountId, CancellationToken ct = default);
    Task SaveAsync(Account account, CancellationToken ct = default);
    Task<PaginatedList<AccountDto>> GetPagedAsync(PaginationParams pagination, CancellationToken ct = default);
}
