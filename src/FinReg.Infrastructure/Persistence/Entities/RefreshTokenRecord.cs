namespace FinReg.Infrastructure.Persistence.Entities;

public class RefreshTokenRecord
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public UserRecord User { get; set; } = null!;
}
