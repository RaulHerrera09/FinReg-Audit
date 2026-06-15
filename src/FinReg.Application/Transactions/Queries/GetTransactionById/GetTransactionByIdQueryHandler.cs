using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using FinReg.Application.Transactions.Queries.GetTransactionsByAccount;
using FinReg.Domain.Exceptions;
using MediatR;

namespace FinReg.Application.Transactions.Queries.GetTransactionById;

public sealed class GetTransactionByIdQueryHandler(ITransactionRepository transactionRepository)
    : IRequestHandler<GetTransactionByIdQuery, ApiResponse<TransactionDto>>
{
    public async Task<ApiResponse<TransactionDto>> Handle(GetTransactionByIdQuery query, CancellationToken ct)
    {
        var tx = await transactionRepository.GetByIdAsync(query.TransactionId, ct)
            ?? throw new InvalidTransactionException(query.TransactionId);

        return ApiResponse<TransactionDto>.Ok(tx);
    }
}
