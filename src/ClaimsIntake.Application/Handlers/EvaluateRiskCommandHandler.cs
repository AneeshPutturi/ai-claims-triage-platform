// =============================================
// Handler: EvaluateRiskCommandHandler
// Description: Orchestrates risk assessment using verified data
// Author: Application Team
// Date: February 2026
// =============================================

using System.Text.Json;
using ClaimsIntake.Application.Commands;
using ClaimsIntake.Application.Interfaces;
using ClaimsIntake.Application.Services;
using ClaimsIntake.Domain.Entities;

namespace ClaimsIntake.Application.Handlers;

/// <summary>
/// Handler for risk evaluation command.
/// Enforces verified-data-only requirement and persists risk assessment.
/// </summary>
public class EvaluateRiskCommandHandler
{
    private readonly IClaimRepository _claimRepository;
    private readonly IRiskAssessmentRepository _riskAssessmentRepository;
    private readonly IRiskEvaluationService _riskEvaluationService;
    private readonly IVerificationGuardService _verificationGuard;
    private readonly IAuditLogService _auditLogService;

    public EvaluateRiskCommandHandler(
        IClaimRepository claimRepository,
        IRiskAssessmentRepository riskAssessmentRepository,
        IRiskEvaluationService riskEvaluationService,
        IVerificationGuardService verificationGuard,
        IAuditLogService auditLogService)
    {
        _claimRepository = claimRepository;
        _riskAssessmentRepository = riskAssessmentRepository;
        _riskEvaluationService = riskEvaluationService;
        _verificationGuard = verificationGuard;
        _auditLogService = auditLogService;
    }

    public async Task<EvaluateRiskResult> HandleAsync(
        EvaluateRiskCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validate claim exists
        var claim = await _claimRepository.GetByIdAsync(command.ClaimId, cancellationToken)
            ?? throw new InvalidOperationException($"Claim {command.ClaimId} not found");

        // Enforce verified-data-only requirement
        // This will throw if any required fields are unverified
        await _verificationGuard.EnsureAllVerifiedAsync(command.ClaimId, cancellationToken);

        // Perform risk evaluation
        var evaluationResult = await _riskEvaluationService.EvaluateRiskAsync(
            command.ClaimId,
            cancellationToken);

        // Serialize signals for persistence
        var ruleSignalsJson = JsonSerializer.Serialize(evaluationResult.RuleSignals);
        var aiSignalsJson = JsonSerializer.Serialize(evaluationResult.AIObservations);

        // Create risk assessment snapshot
        var riskAssessment = RiskAssessment.Create(
            command.ClaimId,
            evaluationResult.RiskLevel,
            ruleSignalsJson,
            aiSignalsJson,
            evaluationResult.OverallScore,
            evaluationResult.ModelUsed);

        // Persist risk assessment
        var riskAssessmentId = await _riskAssessmentRepository.InsertAsync(
            riskAssessment,
            cancellationToken);

        // Emit audit log
        await _auditLogService.LogRiskAssessedAsync(
            command.ClaimId,
            claim.ClaimNumber.Value,
            evaluationResult.RiskLevel.ToString(),
            evaluationResult.RuleSignals.Count(r => r.Triggered),
            evaluationResult.AIObservations.Count,
            cancellationToken);

        return new EvaluateRiskResult
        {
            Success = true,
            Message = $"Risk assessed as {evaluationResult.RiskLevel}",
            RiskAssessmentId = riskAssessmentId,
            RiskLevel = evaluationResult.RiskLevel.ToString(),
            OverallScore = evaluationResult.OverallScore
        };
    }
}

/// <summary>
/// Result of risk evaluation command.
/// </summary>
public class EvaluateRiskResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid RiskAssessmentId { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public decimal OverallScore { get; set; }
}
