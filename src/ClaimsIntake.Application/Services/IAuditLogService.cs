// =============================================
// Service Interface: IAuditLogService
// Description: Audit logging service for state-changing operations
// Author: Application Team
// Date: February 2026
// =============================================

namespace ClaimsIntake.Application.Services;

/// <summary>
/// Service for recording audit events.
/// Every state-changing operation must emit an audit event.
/// </summary>
public interface IAuditLogService
{
    Task LogClaimSubmittedAsync(
        Guid claimId, 
        string claimNumber, 
        string actor, 
        CancellationToken cancellationToken = default);

    Task LogPolicyValidatedAsync(
        Guid claimId, 
        string claimNumber, 
        string coverageStatus, 
        CancellationToken cancellationToken = default);

    Task LogClaimVerifiedAsync(
        Guid claimId, 
        string claimNumber, 
        string actor, 
        CancellationToken cancellationToken = default);

    Task LogClaimTriagedAsync(
        Guid claimId, 
        string claimNumber, 
        string riskLevel, 
        CancellationToken cancellationToken = default);

    Task LogDocumentUploadedAsync(
        Guid claimId,
        string claimNumber,
        Guid documentId,
        string fileName,
        string actor,
        CancellationToken cancellationToken = default);

    Task LogDocumentAccessedAsync(
        Guid claimId,
        string claimNumber,
        Guid documentId,
        string fileName,
        string actor,
        CancellationToken cancellationToken = default);

    Task LogAIExtractionPerformedAsync(
        Guid claimId,
        string claimNumber,
        Guid documentId,
        string fileName,
        string modelName,
        int tokensUsed,
        int fieldsExtracted,
        string actor,
        CancellationToken cancellationToken = default);

    Task LogFieldVerifiedAsync(
        Guid claimId,
        string claimNumber,
        Guid extractedFieldId,
        string fieldName,
        string actionTaken,
        string actor,
        CancellationToken cancellationToken = default);

    Task LogRiskAssessedAsync(
        Guid claimId,
        string claimNumber,
        string riskLevel,
        int ruleTriggersCount,
        int aiObservationsCount,
        CancellationToken cancellationToken = default);

    Task LogClaimTriagedAsync(
        Guid claimId,
        string claimNumber,
        string riskLevel,
        string queue,
        CancellationToken cancellationToken = default);

    Task LogTriageOverriddenAsync(
        Guid claimId,
        string claimNumber,
        string queue,
        string overrideBy,
        string overrideReason,
        CancellationToken cancellationToken = default);
}
