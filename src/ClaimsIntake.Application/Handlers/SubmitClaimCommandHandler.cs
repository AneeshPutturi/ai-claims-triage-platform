// =============================================
// Command Handler: SubmitClaimCommandHandler
// Description: Handles FNOL submission with atomic persistence
// Author: Application Team
// Date: February 2026
// =============================================
// Purpose: Coordinates domain logic and persistence.
// No HTTP logic, no framework dependencies.
// Every write emits audit event.
// =============================================

using ClaimsIntake.Application.Commands;
using ClaimsIntake.Application.Interfaces;
using ClaimsIntake.Application.Services;
using ClaimsIntake.Domain.Entities;
using ClaimsIntake.Domain.Enums;
using ClaimsIntake.Domain.ValueObjects;

namespace ClaimsIntake.Application.Handlers;

/// <summary>
/// Handles claim submission command.
/// Coordinates claim creation, policy validation, and audit logging.
/// </summary>
public class SubmitClaimCommandHandler
{
    private readonly IClaimRepository _claimRepository;
    private readonly IPolicySnapshotRepository _snapshotRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IPolicyValidationService _policyValidationService;

    public SubmitClaimCommandHandler(
        IClaimRepository claimRepository,
        IPolicySnapshotRepository snapshotRepository,
        IAuditLogService auditLogService,
        IPolicyValidationService policyValidationService)
    {
        _claimRepository = claimRepository;
        _snapshotRepository = snapshotRepository;
        _auditLogService = auditLogService;
        _policyValidationService = policyValidationService;
    }

    /// <summary>
    /// Handle claim submission.
    /// Returns claim ID on success.
    /// </summary>
    public async Task<Guid> HandleAsync(SubmitClaimCommand command, CancellationToken cancellationToken = default)
    {
        // Validate command
        command.Validate();

        // Generate claim number
        var sequenceNumber = await _claimRepository.GetNextSequenceNumberAsync(cancellationToken);
        var claimNumber = ClaimNumber.Generate(sequenceNumber);

        // Create value objects
        var policyId = PolicyId.From(command.PolicyNumber);
        var lossDate = LossDate.From(command.LossDate);

        // Create claim entity
        var claim = Claim.Create(
            claimNumber: claimNumber,
            policyNumber: policyId,
            lossDate: lossDate,
            lossType: command.LossType,
            lossLocation: command.LossLocation,
            lossDescription: command.LossDescription,
            submittedBy: command.SubmittedBy);

        // Validate policy and create snapshot
        var policySnapshot = await _policyValidationService.ValidateAndCreateSnapshotAsync(
            claim.ClaimId,
            policyId,
            lossDate,
            cancellationToken);

        // Persist claim and snapshot atomically
        // Both must succeed or both must fail
        await _claimRepository.AddAsync(claim, cancellationToken);
        await _snapshotRepository.AddAsync(policySnapshot, cancellationToken);

        // Emit audit event
        await _auditLogService.LogClaimSubmittedAsync(
            claimId: claim.ClaimId,
            claimNumber: claimNumber.Value,
            actor: command.SubmittedBy,
            cancellationToken: cancellationToken);

        // Mark claim as validated if policy was in force
        if (policySnapshot.WasInForceOn(lossDate.Value))
        {
            claim.MarkAsValidated();
            await _claimRepository.UpdateAsync(claim, cancellationToken);

            await _auditLogService.LogPolicyValidatedAsync(
                claimId: claim.ClaimId,
                claimNumber: claimNumber.Value,
                coverageStatus: policySnapshot.CoverageStatus.ToString(),
                cancellationToken: cancellationToken);
        }

        return claim.ClaimId;
    }
}
