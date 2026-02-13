// =============================================
// Handler: ExtractClaimDataCommandHandler
// Description: Orchestrates AI extraction and persistence
// Author: Application Team
// Date: February 2026
// =============================================

using ClaimsIntake.Application.Commands;
using ClaimsIntake.Application.Interfaces;
using ClaimsIntake.Application.Services;
using ClaimsIntake.Domain.Entities;

namespace ClaimsIntake.Application.Handlers;

/// <summary>
/// Handler for AI extraction command.
/// Coordinates document retrieval, AI invocation, validation, and persistence.
/// </summary>
public class ExtractClaimDataCommandHandler
{
    private readonly IClaimRepository _claimRepository;
    private readonly IClaimDocumentRepository _documentRepository;
    private readonly IExtractedFieldRepository _extractedFieldRepository;
    private readonly IExtractionService _extractionService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IAuditLogService _auditLogService;

    public ExtractClaimDataCommandHandler(
        IClaimRepository claimRepository,
        IClaimDocumentRepository documentRepository,
        IExtractedFieldRepository extractedFieldRepository,
        IExtractionService extractionService,
        IBlobStorageService blobStorageService,
        IAuditLogService auditLogService)
    {
        _claimRepository = claimRepository;
        _documentRepository = documentRepository;
        _extractedFieldRepository = extractedFieldRepository;
        _extractionService = extractionService;
        _blobStorageService = blobStorageService;
        _auditLogService = auditLogService;
    }

    public async Task<ExtractClaimDataResult> HandleAsync(
        ExtractClaimDataCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validate claim exists
        var claim = await _claimRepository.GetByIdAsync(command.ClaimId, cancellationToken)
            ?? throw new InvalidOperationException($"Claim {command.ClaimId} not found");

        // Validate document exists and belongs to claim
        var document = await _documentRepository.GetByIdAsync(command.DocumentId, cancellationToken)
            ?? throw new InvalidOperationException($"Document {command.DocumentId} not found");

        if (document.ClaimId != command.ClaimId)
            throw new InvalidOperationException("Document does not belong to the specified claim");

        // Check if extraction already exists (idempotency)
        var existingFields = await _extractedFieldRepository.GetByDocumentIdAsync(
            command.DocumentId, 
            cancellationToken);
        
        if (existingFields.Any())
        {
            return new ExtractClaimDataResult
            {
                Success = true,
                Message = "Extraction already exists for this document",
                ExtractedFieldIds = existingFields.Select(f => f.ExtractedFieldId).ToList()
            };
        }

        // Download document content from blob storage
        string documentContent;
        using (var stream = await _blobStorageService.DownloadDocumentAsync(
            document.StorageLocation, 
            cancellationToken))
        using (var reader = new StreamReader(stream))
        {
            documentContent = await reader.ReadToEndAsync();
        }

        // Perform AI extraction
        var extractionResult = await _extractionService.ExtractFromDocumentAsync(
            command.ClaimId,
            command.DocumentId,
            documentContent,
            cancellationToken);

        // Persist extracted fields
        var extractedFieldIds = new List<Guid>();
        foreach (var fieldData in extractionResult.Fields)
        {
            var extractedField = ExtractedField.Create(
                command.ClaimId,
                command.DocumentId,
                fieldData.FieldName,
                fieldData.FieldValue,
                fieldData.ConfidenceScore,
                extractionResult.ModelName,
                extractionResult.SystemPromptVersion,
                extractionResult.UserPromptVersion,
                extractionResult.SchemaVersion);

            var fieldId = await _extractedFieldRepository.InsertAsync(extractedField, cancellationToken);
            extractedFieldIds.Add(fieldId);
        }

        // Emit audit log
        await _auditLogService.LogAIExtractionPerformedAsync(
            command.ClaimId,
            claim.ClaimNumber.Value,
            command.DocumentId,
            document.FileName,
            extractionResult.ModelName,
            extractionResult.TokensUsed,
            extractedFieldIds.Count,
            command.Actor,
            cancellationToken);

        return new ExtractClaimDataResult
        {
            Success = true,
            Message = $"Extracted {extractedFieldIds.Count} fields from document",
            ExtractedFieldIds = extractedFieldIds
        };
    }
}

/// <summary>
/// Result of extraction command.
/// </summary>
public class ExtractClaimDataResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<Guid> ExtractedFieldIds { get; set; } = new();
}
