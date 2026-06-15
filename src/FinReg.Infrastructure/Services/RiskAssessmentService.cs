using FinReg.Application.Common.Interfaces;
using FinReg.Application.Common.Models;
using FinReg.Domain.Enums;

namespace FinReg.Infrastructure.Services;

public sealed class RiskAssessmentService : IRiskAssessmentService
{
    private const decimal CriticalThreshold = 50_000m;
    private const decimal HighThreshold = 25_000m;
    private const decimal MediumThreshold = 10_000m;

    public RiskAssessmentResult Assess(decimal amount, string currency)
    {
        if (amount >= CriticalThreshold)
            return new RiskAssessmentResult(true, RiskLevel.Critical, AlertSeverity.Critical,
                $"Transaction of {currency} {amount:N2} exceeds critical FCA threshold of {currency} {CriticalThreshold:N0}");

        if (amount >= HighThreshold)
            return new RiskAssessmentResult(true, RiskLevel.High, AlertSeverity.High,
                $"Transaction of {currency} {amount:N2} exceeds high-risk FCA threshold of {currency} {HighThreshold:N0}");

        if (amount >= MediumThreshold)
            return new RiskAssessmentResult(true, RiskLevel.Medium, AlertSeverity.Warning,
                $"Transaction of {currency} {amount:N2} exceeds medium-risk FCA threshold of {currency} {MediumThreshold:N0}");

        return new RiskAssessmentResult(false, RiskLevel.Low, AlertSeverity.Info, string.Empty);
    }
}
