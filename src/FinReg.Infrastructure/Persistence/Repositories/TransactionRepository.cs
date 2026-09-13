using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using FinReg.Application.Transactions.Queries.GetTransactionsByAccount;
using Microsoft.EntityFrameworkCore;

namespace FinReg.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository(AppDbContext db) : ITransactionRepository
{
    public async Task<TransactionDto?> GetByIdAsync(Guid transactionId, CancellationToken ct = default)
    {
        var t = await db.Transactions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == transactionId, ct);
        if (t is null) return null;

        return new TransactionDto
        {
            Id = t.Id,
            AccountId = t.AccountId,
            Amount = t.Amount,
            Currency = t.Currency,
            Type = t.Type,
            Status = t.Status,
            Description = t.Description,
            RiskLevel = t.RiskLevel,
            InitiatedAt = t.InitiatedAt,
            CompletedAt = t.CompletedAt,
            FailureReason = t.FailureReason
        };
    }

    public async Task<PaginatedList<TransactionDto>> GetByAccountIdAsync(
        Guid accountId, PaginationParams pagination, CancellationToken ct = default)
    {
        var query = db.Transactions.AsNoTracking()
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.InitiatedAt)
            .ThenByDescending(t => t.Id);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                AccountId = t.AccountId,
                Amount = t.Amount,
                Currency = t.Currency,
                Type = t.Type,
                Status = t.Status,
                Description = t.Description,
                RiskLevel = t.RiskLevel,
                InitiatedAt = t.InitiatedAt,
                CompletedAt = t.CompletedAt,
                FailureReason = t.FailureReason
            })
            .ToListAsync(ct);

        return new PaginatedList<TransactionDto>
        {
            Items = items,
            TotalCount = total,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<IReadOnlyList<TransactionDto>> GetAllByAccountIdAsync(
        Guid accountId, CancellationToken ct = default)
    {
        return await db.Transactions.AsNoTracking()
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.InitiatedAt)
            .ThenByDescending(t => t.Id)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                AccountId = t.AccountId,
                Amount = t.Amount,
                Currency = t.Currency,
                Type = t.Type,
                Status = t.Status,
                Description = t.Description,
                RiskLevel = t.RiskLevel,
                InitiatedAt = t.InitiatedAt,
                CompletedAt = t.CompletedAt,
                FailureReason = t.FailureReason
            })
            .ToListAsync(ct);
    }
}
