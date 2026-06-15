using FinReg.Application.Auth.Commands.Login;
using FinReg.Application.Common.Models;
using MediatR;

namespace FinReg.Application.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(Guid UserId, string RefreshToken) : IRequest<ApiResponse<LoginResponseDto>>;
