using FinReg.Application.Transactions.Queries.GetTransactionById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinReg.API.Extensions;

namespace FinReg.API.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize(Policy = AuthExtensions.ReadAccess)]
public sealed class TransactionsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}", Name = "GetTransaction")]
    public async Task<IActionResult> GetTransaction(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTransactionByIdQuery(id), ct);
        return Ok(result);
    }
}
