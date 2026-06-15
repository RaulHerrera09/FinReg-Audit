using FinReg.Application.Common.Models;

namespace FinReg.Application.Common.Interfaces;

public interface IRiskAssessmentService
{
    RiskAssessmentResult Assess(decimal amount, string currency);
}
