using FinReg.Application.Common.Models;
using MediatR;

namespace FinReg.Application.Reports.Queries.GetComplianceReport;

public sealed record GetComplianceReportQuery(Guid AccountId) : IRequest<ApiResponse<ComplianceReportDto>>;
