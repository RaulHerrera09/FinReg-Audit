using FinReg.Application.Common.Models;
using MediatR;

namespace FinReg.Application.Accounts.Commands.SuspendAccount;

public sealed record SuspendAccountCommand(Guid AccountId, string Reason) : IRequest<ApiResponse<bool>>;
