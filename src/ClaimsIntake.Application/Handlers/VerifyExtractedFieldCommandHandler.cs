// =============================================
// Handler: VerifyExtractedFieldCommandHandler
// Description: Orchestrates human verification of AI-extracted data
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
/// Handler for verification command.
/// Enforces single-action rule and records verification decision.
/// </summary>
public class VerifyExtractedFieldCommandHandler
{
    private readonly IExtractedFieldRepository _extractedFieldRepository;
    private readonly IVerificationRecordRepository _verificationRecordRepository;
    private readonly IClaimRepository _claimRepository;
    private readonly IAuditLogService _auditLogService;

    public VerifyExtractedFieldCommandHandler(
        IExtractedFieldRepository extractedFieldRepository,
        IVerificationRecordRepository verificationRecordRepository,
        IClaimRepository claimRepository,
        IAuditLogService auditLogService)
    {
        _extractedFieldRepository = extractedFieldRepository;
        _verificationRecordRepository = verificationRecordRepository;
        _claimRepository = claimRepository;
        _auditLogService = auditLogService;
    }

    public async Task<VerifyExtractedFieldResult> HandleAsync(
        VerifyExtractedFieldCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validate extracted field exists
        var extractedField = await _extractedFieldRepository.GetByIdAsync(
            command.ExtractedFieldId, 
            cancellationToken)
            ?? throw new InvalidOperationException(
                $"Extracted field {command.ExtractedFieldId} not found");

        // Enforce single-action rule: field can only be verified once
        var existingVerification = await _verificationRecordRepository.ExistsForFieldAsync(
            command.ExtractedFieldId,
            cancellationToken);

        if (existingVerification)
        {
            throw new InvalidOperationException(
                $"Extracted field {command.ExtractedFieldId} has already been verified. " +
                "Verification decisions are immutable and cannot be changed.");
        }

        // Validate field is in Unverified state
        if (extractedField.VerificationStatus != VerificationStatus.Unverified)
        {
            throw new InvalidOperationException(
                $"Extracted field {command.ExtractedFieldId} is not in Unverified state. " +
                $"Current status: {extractedField.VerificationStatus}");
        }

        // Get claim for audit logging
        var claim = await _claimRepository.GetByIdAsync(extractedField.ClaimId, cancellationToken)
            ?? throw new InvalidOperationException($"Claim {extractedField.ClaimId} not found");

        // Create verification record
        var verificationRecord = VerificationRecord.Create(
            extractedField.ClaimId,
            command.ExtractedFieldId,
            command.VerifiedBy,
            command.ActionTaken,
            command.CorrectedValue,
            command.VerificationNotes);

        // Persist verification record
        var verificationId = await _verificationRecordRepository.InsertAsync(
            verificationRecord, 
            cancellationToken);

        // Update extracted field verification status
        var newStatus = command.ActionTaken switch
        {
            "Accepted" => VerificationStatus.Verified.ToString(),
            "Corrected" => VerificationStatus.Corrected.ToString(),
            "Rejected" => VerificationStatus.Rejected.ToString(),
            _ => throw new InvalidOperationException($"Invalid action: {command.ActionTaken}")
        };

        await _extractedFieldRepository.UpdateVerificationStatusAsync(
            command.ExtractedFieldId,
            newStatus,
            cancellationToken);

        // Emit audit log
        await _auditLogService.LogFieldVerifiedAsync(
            extractedField.ClaimId,
            claim.ClaimNumber.Value,
            command.ExtractedFieldId,
            extractedField.FieldName,
            command.ActionTaken,
            command.VerifiedBy,
            cancellationToken);

        return new VerifyExtractedFieldResult
        {
            Success = true,
            Message = $"Field verified with action: {command.ActionTaken}",
            VerificationId = verificationId
        };
    }
}

/// <summary>
/// Result of verification command.
/// </summary>
public class VerifyExtractedFieldResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid VerificationId { get; set; }
}
