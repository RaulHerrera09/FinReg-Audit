using FinReg.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinReg.Infrastructure.Persistence.Configurations;

public class AccountSnapshotConfiguration : IEntityTypeConfiguration<AccountSnapshotRecord>
{
    public void Configure(EntityTypeBuilder<AccountSnapshotRecord> builder)
    {
        builder.ToTable("account_snapshots");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
        builder.Property(e => e.StateJson).HasColumnName("state_json").HasColumnType("jsonb").IsRequired();
        builder.Property(e => e.Version).HasColumnName("version").IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.HasIndex(e => new { e.AccountId, e.Version }).HasDatabaseName("ix_account_snapshots_account_version");
    }
}
