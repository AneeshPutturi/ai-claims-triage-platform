// =============================================
// Handler: TriageClaimCommandHandler
// Description: Orchestrates claim routing based on risk assessment
// Author: Application Team
// Date: February 2026
// =============================================

using ClaimsIntake.Application.Commands;
using ClaimsIntake.Application.Interfaces;
using ClaimsIntake.Application.Services;
using ClaimsIntake.Domain.Entities;
using ClaimsIntake.Domain.Enums;

namespace ClaimsIntake.Application.Handlers;

/// <summary>
/// Handler for triage command.
/// Routes claims based on deterministic rules and latest risk assessment.
/// </summary>
public class TriageClaimCommandHandler
{
    private readonly IClaimRepository _claimRepository;
    private readonly IRiskAssessmentRepository _riskAssessmentRepository;
    private readonly ITriageDecisionRepository _triageDecisionRepository;
    private readonly IAuditLogService _auditLogService;

    public TriageClaimCommandHandler(
        IClaimRepository claimRepository,
        IRiskAssessmentRepository riskAssessmentRepository,
        ITriageDecisionRepository triageDecisionRepository,
        IAuditLogService auditLogService)
    {
        _claimRepository = claimRepository;
        _riskAssessmentRepository = riskAssessmentRepository;
        _triageDecisionRepository = triageDecisionRepository;
        _auditLogService = auditLogService;
    }

    public async Task<TriageClaimResult> HandleAsync(
        TriageClaimCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validate claim exists
        var claim = await _claimRepository.GetByIdAsync(command.ClaimId, cancellationToken)
            ?? throw new InvalidOperationException($"Claim {command.ClaimId} not found");

        // Get latest risk assessment
        var riskAssessment = await _riskAssessmentRepository.GetLatestByClaimIdAsync(
            command.ClaimId, 
            cancellationToken)
            ?? throw new InvalidOperationException(
                $"No risk assessment found for claim {command.ClaimId}. " +
                "Risk assessment must be performed before triage.");

        // Check for existing routing decision for this risk assessment (idempotency)
        var existingDecision = await _triageDecisionRepository.ExistsForRiskAssessmentAsync(
            command.ClaimId,
            riskAssessment.RiskAssessmentId,
            cancellationToken);

        if (existingDecision)
        {
            var existing = await _triageDecisionRepository.GetLatestByClaimIdAsync(
                command.ClaimId,
                cancellationToken);
            
            return new TriageClaimResult
            {
                Success = true,
                Message = "Claim already triaged for this risk assessment",
                TriageDecisionId = existing!.TriageDecisionId,
                Queue = existing.Queue
            };
        }

        // Apply deterministic routing rules
        var queue = DetermineQueue(riskAssessment.RiskLevel);

        // Create triage decision
        var triageDecision = TriageDecision.Create(
            command.ClaimId,
            riskAssessment.RiskAssessmentId,
            queue);

        // Persist triage decision
        var triageDecisionId = await _triageDecisionRepository.InsertAsync(
            triageDecision,
            cancellationToken);

        // Update claim status to Triaged
        await _claimRepository.UpdateStatusAsync(
            command.ClaimId,
            ClaimStatus.Triaged.ToString(),
            cancellationToken);

        // Emit audit log
        await _auditLogService.LogClaimTriagedAsync(
            command.ClaimId,
            claim.ClaimNumber.Value,
            riskAssessment.RiskLevel.ToString(),
            queue,
            cancellationToken);

        return new TriageClaimResult
        {
            Success = true,
            Message = $"Claim triaged to {queue} queue",
            TriageDecisionId = triageDecisionId,
            Queue = queue
        };
    }

    private string DetermineQueue(RiskLevel riskLevel)
    {
        return riskLevel switch
        {
            RiskLevel.Low => "Auto-Review",
            RiskLevel.Medium => "Standard Review",
            RiskLevel.High => "Manual Investigation",
            _ => throw new InvalidOperationException($"Unknown risk level: {riskLevel}")
        };
    }
}

/// <summary>
/// Result of triage command.
/// </summary>
public class TriageClaimResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid TriageDecisionId { get; set; }
    public string Queue { get; set; } = string.Empty;
}
