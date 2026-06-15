using FinReg.Application.Auth.Commands.Login;
using FinReg.Application.Auth.Commands.RefreshToken;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinReg.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    public sealed record LoginRequest(string Email, string Password);
    public sealed record RefreshTokenRequest(Guid UserId, string RefreshToken);

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new LoginCommand(request.Email, request.Password), ct);
        if (!result.Success) return Unauthorized(result);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new RefreshTokenCommand(request.UserId, request.RefreshToken), ct);
        if (!result.Success) return Unauthorized(result);
        return Ok(result);
    }
}
