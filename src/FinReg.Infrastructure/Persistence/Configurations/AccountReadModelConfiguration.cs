using FinReg.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinReg.Infrastructure.Persistence.Configurations;

public class AccountReadModelConfiguration : IEntityTypeConfiguration<AccountReadModel>
{
    public void Configure(EntityTypeBuilder<AccountReadModel> builder)
    {
        builder.ToTable("accounts");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.AccountNumber).HasColumnName("account_number").HasMaxLength(20).IsRequired();
        builder.Property(e => e.HolderName).HasColumnName("holder_name").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(e => e.Balance).HasColumnName("balance").HasPrecision(18, 2).IsRequired();
        builder.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(e => e.RiskLevel).HasColumnName("risk_level").HasMaxLength(20).IsRequired();
        builder.Property(e => e.Version).HasColumnName("version").IsRequired();
        builder.Property(e => e.LastUpdated).HasColumnName("last_updated").IsRequired();
        builder.HasIndex(e => e.AccountNumber).IsUnique().HasDatabaseName("ix_accounts_account_number");
    }
}
