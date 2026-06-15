using FinReg.Application.Common.Models;
using MediatR;

namespace FinReg.Application.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<ApiResponse<LoginResponseDto>>;
