namespace FinReg.Infrastructure.Persistence.Entities;

public class UserRecord
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public ICollection<RefreshTokenRecord> RefreshTokens { get; set; } = [];
}
