using FinReg.Domain.Enums;

namespace FinReg.Application.Common.Models;

public sealed record RiskAssessmentResult(
    bool ShouldFlag,
    RiskLevel RiskLevel,
    AlertSeverity Severity,
    string Reason);
