// =============================================
// Service Implementation: AuditLogService
// Description: Audit logging for state-changing operations
// Author: Infrastructure Team
// Date: February 2026
// =============================================

using ClaimsIntake.Application.Interfaces;
using ClaimsIntake.Application.Services;
using ClaimsIntake.Domain.Entities;

namespace ClaimsIntake.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;

    public AuditLogService(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task LogClaimSubmittedAsync(
        Guid claimId, 
        string claimNumber, 
        string actor, 
        CancellationToken cancellationToken = default)
    {
        var auditEvent = AuditEvent.Create(
            actor: actor,
            action: "ClaimSubmitted",
            entityType: "Claim",
            entityId: claimNumber,
            outcome: "Success",
            details: $"{{\"ClaimId\":\"{claimId}\"}}");

        await _repository.AddAsync(auditEvent, cancellationToken);
    }

    public async Task LogPolicyValidatedAsync(
        Guid claimId, 
        string claimNumber, 
        string coverageStatus, 
        CancellationToken cancellationToken = default)
    {
        var auditEvent = AuditEvent.Create(
            actor: "System",
            action: "PolicyValidated",
            entityType: "Claim",
            entityId: claimNumber,
            outcome: "Success",
            details: $"{{\"ClaimId\":\"{claimId}\",\"CoverageStatus\":\"{coverageStatus}\"}}");

        await _repository.AddAsync(auditEvent, cancellationToken);
    }

    public async Task LogClaimVerifiedAsync(
        Guid claimId, 
        string claimNumber, 
        string actor, 
        CancellationToken cancellationToken = default)
    {
        var auditEvent = AuditEvent.Create(
            actor: actor,
            action: "ClaimVerified",
            entityType: "Claim",
            entityId: claimNumber,
            outcome: "Success",
            details: $"{{\"ClaimId\":\"{claimId}\"}}");

        await _repository.AddAsync(auditEvent, cancellationToken);
    }

    public async Task LogClaimTriagedAsync(
        Guid claimId, 
        string claimNumber, 
        string riskLevel, 
        CancellationToken cancellationToken = default)
    {
        var auditEvent = AuditEvent.Create(
            actor: "System",
            action: "ClaimTriaged",
            entityType: "Claim",
            entityId: claimNumber,
            outcome: "Success",
            details: $"{{\"ClaimId\":\"{claimId}\",\"RiskLevel\":\"{riskLevel}\"}}");

        await _repository.AddAsync(auditEvent, cancellationToken);
    }

    public async Task LogDocumentUploadedAsync(
        Guid claimId,
        string claimNumber,
        Guid documentId,
        string fileName,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var auditEvent = AuditEvent.Create(
            actor: actor,
            action: "DocumentUploaded",
            entityType: "Document",
            entityId: documentId.ToString(),
            outcome: "Success",
            details: $"{{\"ClaimId\":\"{claimId}\",\"ClaimNumber\":\"{claimNumber}\",\"FileName\":\"{fileName}\"}}");

        await _repository.AddAsync(auditEvent, cancellationToken);
    }

    public async Task LogDocumentAccessedAsync(
        Guid claimId,
        string claimNumber,
        Guid documentId,
        string fileName,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var auditEvent = AuditEvent.Create(
            actor: actor,
            action: "DocumentAccessed",
            entityType: "Document",
            entityId: documentId.ToString(),
            outcome: "Success",
            details: $"{{\"ClaimId\":\"{claimId}\",\"ClaimNumber\":\"{claimNumber}\",\"FileName\":\"{fileName}\"}}");

        await _repository.AddAsync(auditEvent, cancellationToken);
    }

    public async Task LogAIExtractionPerformedAsync(
        Guid claimId,
        string claimNumber,
        Guid documentId,
        string fileName,
        string modelName,
        int tokensUsed,
        int fieldsExtracted,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var auditEvent = AuditEvent.Create(
            actor: actor,
            action: "AIExtractionPerformed",
            entityType: "ExtractedField",
            entityId: documentId.ToString(),
            outcome: "Success",
            details: $"{{\"ClaimId\":\"{claimId}\",\"ClaimNumber\":\"{claimNumber}\",\"DocumentId\":\"{documentId}\",\"FileName\":\"{fileName}\",\"ModelName\":\"{modelName}\",\"TokensUsed\":{tokensUsed},\"FieldsExtracted\":{fieldsExtracted}}}");

        await _repository.AddAsync(auditEvent, cancellationToken);
    }

    public async Task LogFieldVerifiedAsync(
        Guid claimId,
        string claimNumber,
        Guid extractedFieldId,
        string fieldName,
        string actionTaken,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var auditEvent = AuditEvent.Create(
            actor: actor,
            action: "FieldVerified",
            entityType: "ExtractedField",
            entityId: extractedFieldId.ToString(),
            outcome: "Success",
            details: $"{{\"ClaimId\":\"{claimId}\",\"ClaimNumber\":\"{claimNumber}\",\"FieldName\":\"{fieldName}\",\"ActionTaken\":\"{actionTaken}\"}}");

        await _repository.AddAsync(auditEvent, cancellationToken);
    }

    public async Task LogRiskAssessedAsync(
        Guid claimId,
        string claimNumber,
        string riskLevel,
        int ruleTriggersCount,
        int aiObservationsCount,
        CancellationToken cancellationToken = default)
    {
        var auditEvent = AuditEvent.Create(
            actor: "System",
            action: "RiskAssessed",
            entityType: "RiskAssessment",
            entityId: claimId.ToString(),
            outcome: "Success",
            details: $"{{\"ClaimId\":\"{claimId}\",\"ClaimNumber\":\"{claimNumber}\",\"RiskLevel\":\"{riskLevel}\",\"RuleTriggersCount\":{ruleTriggersCount},\"AIObservationsCount\":{aiObservationsCount}}}");

        await _repository.AddAsync(auditEvent, cancellationToken);
    }

    public async Task LogClaimTriagedAsync(
        Guid claimId,
        string claimNumber,
        string riskLevel,
        string queue,
        CancellationToken cancellationToken = default)
    {
        var auditEvent = AuditEvent.Create(
            actor: "System",
            action: "ClaimTriaged",
            entityType: "TriageDecision",
            entityId: claimId.ToString(),
            outcome: "Success",
            details: $"{{\"ClaimId\":\"{claimId}\",\"ClaimNumber\":\"{claimNumber}\",\"RiskLevel\":\"{riskLevel}\",\"Queue\":\"{queue}\"}}");

        await _repository.AddAsync(auditEvent, cancellationToken);
    }

    public async Task LogTriageOverriddenAsync(
        Guid claimId,
        string claimNumber,
        string queue,
        string overrideBy,
        string overrideReason,
        CancellationToken cancellationToken = default)
    {
        var auditEvent = AuditEvent.Create(
            actor: overrideBy,
            action: "TriageOverridden",
            entityType: "TriageDecision",
            entityId: claimId.ToString(),
            outcome: "Success",
            details: $"{{\"ClaimId\":\"{claimId}\",\"ClaimNumber\":\"{claimNumber}\",\"Queue\":\"{queue}\",\"OverrideReason\":\"{overrideReason}\"}}");

        await _repository.AddAsync(auditEvent, cancellationToken);
    }
}
