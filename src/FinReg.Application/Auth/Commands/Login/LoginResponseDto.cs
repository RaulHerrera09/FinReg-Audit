namespace FinReg.Application.Auth.Commands.Login;

public sealed record LoginResponseDto
{
    public string AccessToken { get; init; } = null!;
    public string RefreshToken { get; init; } = null!;
    public DateTime ExpiresAt { get; init; }
    public string UserId { get; init; } = null!;
    public string Role { get; init; } = null!;
}
