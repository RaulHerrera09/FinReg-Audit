using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using FinReg.Domain.Exceptions;
using MediatR;

namespace FinReg.Application.Reports.Queries.GetComplianceReport;

public sealed class GetComplianceReportQueryHandler(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository)
    : IRequestHandler<GetComplianceReportQuery, ApiResponse<ComplianceReportDto>>
{
    public async Task<ApiResponse<ComplianceReportDto>> Handle(GetComplianceReportQuery query, CancellationToken ct)
    {
        var account = await accountRepository.GetByIdAsync(query.AccountId, ct)
            ?? throw new AccountNotFoundException(query.AccountId);

        var transactions = await transactionRepository.GetAllByAccountIdAsync(query.AccountId, ct);

        var flagged = transactions.Count(t => t.Status == "Flagged");
        var totalValue = transactions
            .Where(t => t.Status == "Completed")
            .Sum(t => t.Amount);

        return ApiResponse<ComplianceReportDto>.Ok(new ComplianceReportDto
        {
            AccountId = account.Id,
            AccountNumber = account.AccountNumber.Value,
            HolderName = account.HolderName,
            RiskLevel = account.RiskLevel.ToString(),
            TotalTransactions = transactions.Count,
            TotalValueGbp = totalValue,
            FlaggedTransactions = flagged,
            GeneratedAt = DateTime.UtcNow
        });
    }
}
