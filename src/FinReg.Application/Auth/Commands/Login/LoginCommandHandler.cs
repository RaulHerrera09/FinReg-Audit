using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using MediatR;

namespace FinReg.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IJwtService jwtService,
    IPasswordHasher passwordHasher)
    : IRequestHandler<LoginCommand, ApiResponse<LoginResponseDto>>
{
    public async Task<ApiResponse<LoginResponseDto>> Handle(LoginCommand command, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(command.Email, ct);
        if (user is null || !passwordHasher.Verify(command.Password, user.PasswordHash))
            return ApiResponse<LoginResponseDto>.Fail("Invalid credentials.");

        var accessToken = jwtService.GenerateAccessToken(user.UserId, user.Email, user.Role);
        var refreshToken = jwtService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddHours(1);

        await userRepository.SaveRefreshTokenAsync(user.UserId, refreshToken, expiresAt, ct);

        return ApiResponse<LoginResponseDto>.Ok(new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            UserId = user.UserId.ToString(),
            Role = user.Role
        });
    }
}
