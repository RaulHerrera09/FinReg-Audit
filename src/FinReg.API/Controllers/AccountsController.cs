using FinReg.Application.Accounts.Commands.CloseAccount;
using FinReg.Application.Accounts.Commands.OpenAccount;
using FinReg.Application.Accounts.Commands.SuspendAccount;
using FinReg.Application.Accounts.Queries.GetAccountById;
using FinReg.Application.Accounts.Queries.GetAccounts;
using FinReg.Application.Alerts.Queries.GetAlertsByAccount;
using FinReg.Application.Common.Models;
using FinReg.Application.Reports.Queries.GetAuditTrail;
using FinReg.Application.Reports.Queries.GetComplianceReport;
using FinReg.Application.Transactions.Commands.CompleteTransaction;
using FinReg.Application.Transactions.Commands.FlagTransaction;
using FinReg.Application.Transactions.Commands.InitiateTransaction;
using FinReg.Application.Transactions.Queries.GetTransactionsByAccount;
using FinReg.Domain.Enums;
using FinReg.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinReg.API.Controllers;

[ApiController]
[Route("api/accounts")]
[Authorize(Policy = AuthExtensions.ReadAccess)]
public sealed class AccountsController(IMediator mediator) : ControllerBase
{
    public sealed record OpenAccountRequest(string HolderName, string Currency);
    public sealed record ReasonRequest(string Reason);
    public sealed record InitiateTransactionRequest(
        decimal Amount, string Currency, TransactionType Type, string Description);
    public sealed record FlagTransactionRequest(
        RiskLevel RiskLevel, AlertSeverity Severity, string Reason);

    [HttpGet]
    public async Task<IActionResult> GetAccounts(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetAccountsQuery(new PaginationParams { Page = page, PageSize = pageSize }), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthExtensions.UnavailableOperation)]
    public async Task<IActionResult> OpenAccount([FromBody] OpenAccountRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new OpenAccountCommand(request.HolderName, request.Currency), ct);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetAccount), new { id = result.Data }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAccount(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAccountByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/suspend")]
    [Authorize(Policy = AuthExtensions.UnavailableOperation)]
    public async Task<IActionResult> SuspendAccount(Guid id, [FromBody] ReasonRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new SuspendAccountCommand(id, request.Reason), ct);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{id:guid}/close")]
    [Authorize(Policy = AuthExtensions.UnavailableOperation)]
    public async Task<IActionResult> CloseAccount(Guid id, [FromBody] ReasonRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CloseAccountCommand(id, request.Reason), ct);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("{id:guid}/transactions")]
    public async Task<IActionResult> GetTransactions(
        Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetTransactionsByAccountQuery(id, new PaginationParams { Page = page, PageSize = pageSize }), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/transactions")]
    [Authorize(Policy = AuthExtensions.UnavailableOperation)]
    public async Task<IActionResult> InitiateTransaction(
        Guid id, [FromBody] InitiateTransactionRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new InitiateTransactionCommand(id, request.Amount, request.Currency, request.Type, request.Description), ct);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction("GetTransaction", "Transactions", new { id = result.Data }, result);
    }

    [HttpPost("{id:guid}/transactions/{transactionId:guid}/complete")]
    [Authorize(Policy = AuthExtensions.UnavailableOperation)]
    public async Task<IActionResult> CompleteTransaction(Guid id, Guid transactionId, CancellationToken ct)
    {
        var result = await mediator.Send(new CompleteTransactionCommand(id, transactionId), ct);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{id:guid}/transactions/{transactionId:guid}/flag")]
    [Authorize(Policy = AuthExtensions.ComplianceReview)]
    public async Task<IActionResult> FlagTransaction(
        Guid id, Guid transactionId, [FromBody] FlagTransactionRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new FlagTransactionCommand(id, transactionId, request.RiskLevel, request.Severity, request.Reason), ct);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("{id:guid}/alerts")]
    public async Task<IActionResult> GetAlerts(
        Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetAlertsByAccountQuery(id, new PaginationParams { Page = page, PageSize = pageSize }), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/audit-trail")]
    public async Task<IActionResult> GetAuditTrail(
        Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetAuditTrailQuery(id, new PaginationParams { Page = page, PageSize = pageSize }), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/compliance-report")]
    public async Task<IActionResult> GetComplianceReport(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetComplianceReportQuery(id), ct);
        return Ok(result);
    }
}
