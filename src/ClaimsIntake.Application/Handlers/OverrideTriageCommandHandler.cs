// =============================================
// Handler: OverrideTriageCommandHandler
// Description: Handles human override of routing decisions
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
/// Handler for triage override command.
/// Allows authorized humans to override routing decisions with justification.
/// </summary>
public class OverrideTriageCommandHandler
{
    private readonly IClaimRepository _claimRepository;
    private readonly IRiskAssessmentRepository _riskAssessmentRepository;
    private readonly ITriageDecisionRepository _triageDecisionRepository;
    private readonly IAuditLogService _auditLogService;

    public OverrideTriageCommandHandler(
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

    public async Task<OverrideTriageResult> HandleAsync(
        OverrideTriageCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validate claim exists
        var claim = await _claimRepository.GetByIdAsync(command.ClaimId, cancellationToken)
            ?? throw new InvalidOperationException($"Claim {command.ClaimId} not found");

        // Get latest risk assessment (override still references risk assessment)
        var riskAssessment = await _riskAssessmentRepository.GetLatestByClaimIdAsync(
            command.ClaimId, 
            cancellationToken)
            ?? throw new InvalidOperationException(
                $"No risk assessment found for claim {command.ClaimId}");

        // Create override triage decision
        var triageDecision = TriageDecision.CreateOverride(
            command.ClaimId,
            riskAssessment.RiskAssessmentId,
            command.Queue,
            command.OverrideBy,
            command.OverrideReason);

        // Persist override decision (original decision preserved)
        var triageDecisionId = await _triageDecisionRepository.InsertAsync(
            triageDecision,
            cancellationToken);

        // Update claim status to Triaged if not already
        if (claim.Status != ClaimStatus.Triaged)
        {
            await _claimRepository.UpdateStatusAsync(
                command.ClaimId,
                ClaimStatus.Triaged.ToString(),
                cancellationToken);
        }

        // Emit audit log for override
        await _auditLogService.LogTriageOverriddenAsync(
            command.ClaimId,
            claim.ClaimNumber.Value,
            command.Queue,
            command.OverrideBy,
            command.OverrideReason,
            cancellationToken);

        return new OverrideTriageResult
        {
            Success = true,
            Message = $"Triage overridden to {command.Queue} queue by {command.OverrideBy}",
            TriageDecisionId = triageDecisionId,
            Queue = command.Queue
        };
    }
}

/// <summary>
/// Result of override triage command.
/// </summary>
public class OverrideTriageResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid TriageDecisionId { get; set; }
    public string Queue { get; set; } = string.Empty;
}
