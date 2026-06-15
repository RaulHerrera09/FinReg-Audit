using FinReg.Infrastructure.Persistence.Configurations;
using FinReg.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinReg.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AuditEventRecord> AuditEvents => Set<AuditEventRecord>();
    public DbSet<AccountSnapshotRecord> AccountSnapshots => Set<AccountSnapshotRecord>();
    public DbSet<AccountReadModel> Accounts => Set<AccountReadModel>();
    public DbSet<TransactionReadModel> Transactions => Set<TransactionReadModel>();
    public DbSet<AlertRecord> Alerts => Set<AlertRecord>();
    public DbSet<UserRecord> Users => Set<UserRecord>();
    public DbSet<RefreshTokenRecord> RefreshTokens => Set<RefreshTokenRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AuditEventConfiguration());
        modelBuilder.ApplyConfiguration(new AccountSnapshotConfiguration());
        modelBuilder.ApplyConfiguration(new AccountReadModelConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionReadModelConfiguration());
        modelBuilder.ApplyConfiguration(new AlertConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
    }
}
