using FinReg.Application.Common.Interfaces;
using FinReg.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinReg.Infrastructure.Persistence;

public sealed class DatabaseSeeder(AppDbContext db, IPasswordHasher hasher)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedUserAsync("compliance@demo.finreg.dev", "Compliance@123", "ComplianceOfficer", ct);
        await SeedUserAsync("auditor@demo.finreg.dev", "Audit@123", "Auditor", ct);
    }

    private async Task SeedUserAsync(string email, string password, string role, CancellationToken ct)
    {
        if (await db.Users.AnyAsync(u => u.Email == email, ct))
            return;

        db.Users.Add(new UserRecord
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = hasher.Hash(password),
            Role = role,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
    }
}
