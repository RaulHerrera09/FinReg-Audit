using FinReg.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinReg.Infrastructure.Persistence.Configurations;

public class TransactionReadModelConfiguration : IEntityTypeConfiguration<TransactionReadModel>
{
    public void Configure(EntityTypeBuilder<TransactionReadModel> builder)
    {
        builder.ToTable("transactions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
        builder.Property(e => e.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
        builder.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(e => e.Type).HasColumnName("type").HasMaxLength(20).IsRequired();
        builder.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(500).IsRequired();
        builder.Property(e => e.RiskLevel).HasColumnName("risk_level").HasMaxLength(20).IsRequired();
        builder.Property(e => e.InitiatedAt).HasColumnName("initiated_at").IsRequired();
        builder.Property(e => e.CompletedAt).HasColumnName("completed_at");
        builder.Property(e => e.FailureReason).HasColumnName("failure_reason").HasMaxLength(500);
        builder.HasIndex(e => e.AccountId).HasDatabaseName("ix_transactions_account_id");
    }
}
