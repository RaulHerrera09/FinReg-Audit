using FinReg.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinReg.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshTokenRecord>
{
    public void Configure(EntityTypeBuilder<RefreshTokenRecord> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.Token).HasColumnName("token").HasMaxLength(500).IsRequired();
        builder.Property(e => e.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(e => e.IsRevoked).HasColumnName("is_revoked").IsRequired();
        builder.HasIndex(e => e.Token).HasDatabaseName("ix_refresh_tokens_token");
        builder.HasIndex(e => e.UserId).HasDatabaseName("ix_refresh_tokens_user_id");
    }
}
