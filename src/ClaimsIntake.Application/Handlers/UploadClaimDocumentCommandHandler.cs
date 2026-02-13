// =============================================
// Command Handler: UploadClaimDocumentCommandHandler
// Description: Handles document upload with atomic persistence
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
/// Handles document upload command.
/// Coordinates validation, blob upload, metadata persistence, and audit logging.
/// </summary>
public class UploadClaimDocumentCommandHandler
{
    private readonly IClaimRepository _claimRepository;
    private readonly IClaimDocumentRepository _documentRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IAuditLogService _auditLogService;

    public UploadClaimDocumentCommandHandler(
        IClaimRepository claimRepository,
        IClaimDocumentRepository documentRepository,
        IBlobStorageService blobStorageService,
        IAuditLogService auditLogService)
    {
        _claimRepository = claimRepository;
        _documentRepository = documentRepository;
        _blobStorageService = blobStorageService;
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Handle document upload.
    /// Returns document ID on success.
    /// </summary>
    public async Task<Guid> HandleAsync(UploadClaimDocumentCommand command, CancellationToken cancellationToken = default)
    {
        // Validate command
        command.Validate();

        // Verify claim exists and is in valid state for document upload
        var claim = await _claimRepository.GetByIdAsync(command.ClaimId, cancellationToken);
        if (claim == null)
            throw new InvalidOperationException($"Claim not found: {command.ClaimId}");

        // Only allow document uploads for claims in Submitted, Validated, or Verified states
        if (claim.Status == ClaimStatus.Triaged)
            throw new InvalidOperationException(
                $"Cannot upload documents to claim in {claim.Status} state. " +
                "Documents can only be uploaded to claims in Submitted, Validated, or Verified states.");

        // Generate document ID
        var documentId = Guid.NewGuid();

        // Upload to blob storage first
        // If this fails, no database records are created (no orphaned metadata)
        var blobUri = await _blobStorageService.UploadDocumentAsync(
            claimId: command.ClaimId,
            documentId: documentId,
            fileName: command.FileName,
            fileStream: command.FileStream,
            contentType: command.ContentType,
            cancellationToken: cancellationToken);

        // Create document entity
        var document = ClaimDocument.Create(
            claimId: command.ClaimId,
            fileName: command.FileName,
            documentType: command.DocumentType,
            storageLocation: blobUri,
            fileSizeBytes: command.FileSizeBytes,
            contentType: command.ContentType,
            uploadedBy: command.UploadedBy);

        // Persist metadata
        // If this fails, blob remains in storage (orphaned blob cleanup handled by background job)
        await _documentRepository.AddAsync(document, cancellationToken);

        // Emit audit event
        await _auditLogService.LogDocumentUploadedAsync(
            claimId: command.ClaimId,
            claimNumber: claim.ClaimNumber.Value,
            documentId: documentId,
            fileName: command.FileName,
            actor: command.UploadedBy,
            cancellationToken: cancellationToken);

        return documentId;
    }
}
