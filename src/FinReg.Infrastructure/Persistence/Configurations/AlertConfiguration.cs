using FinReg.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinReg.Infrastructure.Persistence.Configurations;

public class AlertConfiguration : IEntityTypeConfiguration<AlertRecord>
{
    public void Configure(EntityTypeBuilder<AlertRecord> builder)
    {
        builder.ToTable("alerts");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.TransactionId).HasColumnName("transaction_id").IsRequired();
        builder.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
        builder.Property(e => e.Severity).HasColumnName("severity").HasMaxLength(20).IsRequired();
        builder.Property(e => e.RiskLevel).HasColumnName("risk_level").HasMaxLength(20).IsRequired();
        builder.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(500).IsRequired();
        builder.Property(e => e.OccurredOn).HasColumnName("occurred_on").IsRequired();
        builder.HasIndex(e => e.AccountId).HasDatabaseName("ix_alerts_account_id");
        builder.HasIndex(e => e.OccurredOn).HasDatabaseName("ix_alerts_occurred_on");
    }
}
