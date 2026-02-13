// =============================================
// Service Interface: IRiskEvaluationService
// Description: Risk assessment using verified data only
// Author: Application Team
// Date: February 2026
// =============================================

using ClaimsIntake.Domain.Enums;

namespace ClaimsIntake.Application.Services;

/// <summary>
/// Service for evaluating claim risk using deterministic rules and AI signals.
/// Only verified data is eligible for risk assessment.
/// Rules first, AI second, humans always accountable.
/// </summary>
public interface IRiskEvaluationService
{
    /// <summary>
    /// Evaluates risk for a claim using verified data only.
    /// Returns risk assessment with rule signals and AI observations.
    /// </summary>
    Task<RiskEvaluationResult> EvaluateRiskAsync(
        Guid claimId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of risk evaluation.
/// </summary>
public class RiskEvaluationResult
{
    public RiskLevel RiskLevel { get; set; }
    public List<RuleSignal> RuleSignals { get; set; } = new();
    public List<AIObservation> AIObservations { get; set; } = new();
    public decimal OverallScore { get; set; }
    public string? ModelUsed { get; set; }
}

/// <summary>
/// Individual rule signal.
/// </summary>
public class RuleSignal
{
    public string RuleName { get; set; } = string.Empty;
    public bool Triggered { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Individual AI observation.
/// </summary>
public class AIObservation
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RelevantField { get; set; } = string.Empty;
}
