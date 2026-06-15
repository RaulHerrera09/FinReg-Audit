namespace FinReg.Application.Common.Models;

public sealed record UserCredentials
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = null!;
    public string PasswordHash { get; init; } = null!;
    public string Role { get; init; } = null!;
}
