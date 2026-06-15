using FinReg.Application.Common.Models;
using MediatR;

namespace FinReg.Application.Accounts.Commands.CloseAccount;

public sealed record CloseAccountCommand(Guid AccountId, string Reason) : IRequest<ApiResponse<bool>>;
