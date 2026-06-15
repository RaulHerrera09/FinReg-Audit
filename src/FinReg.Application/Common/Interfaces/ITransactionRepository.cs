using FinReg.Application.Common.Models;
using FinReg.Application.Transactions.Queries.GetTransactionsByAccount;

namespace FinReg.Application.Common.Interfaces;

public interface ITransactionRepository
{
    Task<TransactionDto?> GetByIdAsync(Guid transactionId, CancellationToken ct = default);
    Task<PaginatedList<TransactionDto>> GetByAccountIdAsync(Guid accountId, PaginationParams pagination, CancellationToken ct = default);
    Task<IReadOnlyList<TransactionDto>> GetAllByAccountIdAsync(Guid accountId, CancellationToken ct = default);
}
