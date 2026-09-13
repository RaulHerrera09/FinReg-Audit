using FinReg.Application.Alerts.Queries.GetAlerts;
using FinReg.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinReg.API.Extensions;

namespace FinReg.API.Controllers;

[ApiController]
[Route("api/alerts")]
[Authorize(Policy = AuthExtensions.ReadAccess)]
public sealed class AlertsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetAlertsQuery(new PaginationParams { Page = page, PageSize = pageSize }), ct);
        return Ok(result);
    }
}
