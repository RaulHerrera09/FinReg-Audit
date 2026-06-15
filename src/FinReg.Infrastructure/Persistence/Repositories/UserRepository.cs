using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using FinReg.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinReg.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<UserCredentials?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

        return user is null ? null : Map(user);
    }

    public async Task<UserCredentials?> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        return user is null ? null : Map(user);
    }

    public async Task SaveRefreshTokenAsync(
        Guid userId, string token, DateTime expiresAt, CancellationToken ct = default)
    {
        var record = new RefreshTokenRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            IsRevoked = false
        };
        await db.RefreshTokens.AddAsync(record, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> ValidateRefreshTokenAsync(
        Guid userId, string token, CancellationToken ct = default)
    {
        var record = await db.RefreshTokens.AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == userId && r.Token == token, ct);

        if (record is null || record.IsRevoked || record.ExpiresAt < DateTime.UtcNow) return false;

        await db.RefreshTokens
            .Where(r => r.UserId == userId && r.Token == token)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsRevoked, true), ct);

        return true;
    }

    private static UserCredentials Map(UserRecord u) => new()
    {
        UserId = u.Id,
        Email = u.Email,
        PasswordHash = u.PasswordHash,
        Role = u.Role
    };
}
