using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using FinReg.Application.Accounts.Queries.GetAccountById;
using FinReg.Domain.Entities;
using FinReg.Infrastructure.Persistence.EventStore;
using Microsoft.EntityFrameworkCore;

namespace FinReg.Infrastructure.Persistence.Repositories;

public sealed class AccountRepository(AppDbContext db, IEventStore eventStore) : IAccountRepository
{
    private const int SnapshotThreshold = 100;

    public async Task<Account?> GetByIdAsync(Guid accountId, CancellationToken ct = default)
    {
        var events = await eventStore.GetEventsAsync(accountId, 0, ct);
        if (events.Count == 0) return null;

        return Account.Rehydrate(events);
    }

    public async Task SaveAsync(Account account, CancellationToken ct = default)
    {
        var domainEvents = account.DomainEvents.ToList();
        if (domainEvents.Count == 0) return;

        var currentVersion = account.Version - domainEvents.Count;
        await eventStore.AppendEventsAsync(account.Id, nameof(Account), domainEvents, currentVersion, ct);

        if (account.Version % SnapshotThreshold == 0)
        {
            var snapshot = new AccountSnapshot
            {
                AccountId = account.Id,
                StateJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    account.Id,
                    AccountNumber = account.AccountNumber.Value,
                    account.HolderName,
                    Status = account.Status.ToString(),
                    Balance = account.Balance.Amount,
                    Currency = account.Balance.Currency,
                    RiskLevel = account.RiskLevel.ToString(),
                    account.Version
                }),
                Version = account.Version,
                CreatedAt = DateTime.UtcNow
            };
            await eventStore.SaveSnapshotAsync(snapshot, ct);
        }

        await db.SaveChangesAsync(ct);
        account.ClearDomainEvents();
    }

    public async Task<PaginatedList<AccountDto>> GetPagedAsync(PaginationParams pagination, CancellationToken ct = default)
    {
        var query = db.Accounts.AsNoTracking().OrderByDescending(a => a.LastUpdated);
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(a => new AccountDto
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                HolderName = a.HolderName,
                Status = a.Status,
                Balance = a.Balance,
                Currency = a.Currency,
                RiskLevel = a.RiskLevel,
                Version = a.Version
            })
            .ToListAsync(ct);

        return new PaginatedList<AccountDto>
        {
            Items = items,
            TotalCount = total,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }
}
