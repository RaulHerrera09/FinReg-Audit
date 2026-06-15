using FinReg.Application.Auth.Commands.Login;
using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using MediatR;

namespace FinReg.Application.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IJwtService jwtService)
    : IRequestHandler<RefreshTokenCommand, ApiResponse<LoginResponseDto>>
{
    public async Task<ApiResponse<LoginResponseDto>> Handle(RefreshTokenCommand command, CancellationToken ct)
    {
        var isValid = await userRepository.ValidateRefreshTokenAsync(command.UserId, command.RefreshToken, ct);
        if (!isValid)
            return ApiResponse<LoginResponseDto>.Fail("Invalid or expired refresh token.");

        var user = await userRepository.GetByIdAsync(command.UserId, ct);
        if (user is null)
            return ApiResponse<LoginResponseDto>.Fail("User not found.");

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
