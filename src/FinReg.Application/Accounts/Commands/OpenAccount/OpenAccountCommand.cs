using FinReg.Application.Common.Models;
using MediatR;

namespace FinReg.Application.Accounts.Commands.OpenAccount;

public sealed record OpenAccountCommand(string HolderName, string Currency) : IRequest<ApiResponse<Guid>>;
