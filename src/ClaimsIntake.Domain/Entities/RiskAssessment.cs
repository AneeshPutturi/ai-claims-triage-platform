// =============================================
// Domain Entity: RiskAssessment
// Description: Risk evaluation snapshot for a claim
// Author: Domain Team
// Date: February 2026
// =============================================

using ClaimsIntake.Domain.Enums;

namespace ClaimsIntake.Domain.Entities;

/// <summary>
/// Represents a risk assessment snapshot for a claim.
/// Risk is a signal, not a verdict. Rules first, AI second, humans always accountable.
/// Immutable once created - risk assessments are historical snapshots.
/// </summary>
public class RiskAssessment
{
    public Guid RiskAssessmentId { get; private set; }
    public Guid ClaimId { get; private set; }
    public RiskLevel RiskLevel { get; private set; }
    public string RuleSignals { get; private set; }
    public string AISignals { get; private set; }
    public decimal OverallScore { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? AssessedByModel { get; private set; }

    private RiskAssessment() { }

    public static RiskAssessment Create(
        Guid claimId,
        RiskLevel riskLevel,
        string ruleSignals,
        string aiSignals,
        decimal overallScore,
        string? assessedByModel = null)
    {
        if (claimId == Guid.Empty)
            throw new ArgumentException("ClaimId cannot be empty", nameof(claimId));
        
        if (string.IsNullOrWhiteSpace(ruleSignals))
            throw new ArgumentException("RuleSignals cannot be empty", nameof(ruleSignals));
        
        if (string.IsNullOrWhiteSpace(aiSignals))
            throw new ArgumentException("AISignals cannot be empty", nameof(aiSignals));
        
        if (overallScore < 0 || overallScore > 100)
            throw new ArgumentException("OverallScore must be between 0 and 100", nameof(overallScore));

        return new RiskAssessment
        {
            RiskAssessmentId = Guid.NewGuid(),
            ClaimId = claimId,
            RiskLevel = riskLevel,
            RuleSignals = ruleSignals,
            AISignals = aiSignals,
            OverallScore = overallScore,
            CreatedAt = DateTime.UtcNow,
            AssessedByModel = assessedByModel
        };
    }
}
