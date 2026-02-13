// =============================================
// Domain Entity: ExtractedField
// Description: AI-extracted data with verification status
// Author: Domain Team
// Date: February 2026
// =============================================

using ClaimsIntake.Domain.Enums;

namespace ClaimsIntake.Domain.Entities;

/// <summary>
/// Represents AI-extracted data from a claim document.
/// All extracted data is unverified by default.
/// AI output is data, not truth.
/// </summary>
public class ExtractedField
{
    public Guid ExtractedFieldId { get; private set; }
    public Guid ClaimId { get; private set; }
    public Guid DocumentId { get; private set; }
    public string FieldName { get; private set; }
    public string? FieldValue { get; private set; }
    public decimal ConfidenceScore { get; private set; }
    public VerificationStatus VerificationStatus { get; private set; }
    public DateTime ExtractedAt { get; private set; }
    public string ExtractedByModel { get; private set; }
    public string SystemPromptVersion { get; private set; }
    public string UserPromptVersion { get; private set; }
    public string SchemaVersion { get; private set; }

    private ExtractedField() { }

    public static ExtractedField Create(
        Guid claimId,
        Guid documentId,
        string fieldName,
        string? fieldValue,
        decimal confidenceScore,
        string extractedByModel,
        string systemPromptVersion,
        string userPromptVersion,
        string schemaVersion)
    {
        if (claimId == Guid.Empty)
            throw new ArgumentException("ClaimId cannot be empty", nameof(claimId));
        
        if (documentId == Guid.Empty)
            throw new ArgumentException("DocumentId cannot be empty", nameof(documentId));
        
        if (string.IsNullOrWhiteSpace(fieldName))
            throw new ArgumentException("FieldName cannot be empty", nameof(fieldName));
        
        if (confidenceScore < 0 || confidenceScore > 1)
            throw new ArgumentException("ConfidenceScore must be between 0 and 1", nameof(confidenceScore));

        return new ExtractedField
        {
            ExtractedFieldId = Guid.NewGuid(),
            ClaimId = claimId,
            DocumentId = documentId,
            FieldName = fieldName,
            FieldValue = fieldValue,
            ConfidenceScore = confidenceScore,
            VerificationStatus = VerificationStatus.Unverified,
            ExtractedAt = DateTime.UtcNow,
            ExtractedByModel = extractedByModel,
            SystemPromptVersion = systemPromptVersion,
            UserPromptVersion = userPromptVersion,
            SchemaVersion = schemaVersion
        };
    }

    public void MarkAsVerified()
    {
        if (VerificationStatus != VerificationStatus.Unverified)
            throw new InvalidOperationException("Field has already been verified");
        
        VerificationStatus = VerificationStatus.Verified;
    }

    public void MarkAsCorrected()
    {
        if (VerificationStatus != VerificationStatus.Unverified)
            throw new InvalidOperationException("Field has already been verified");
        
        VerificationStatus = VerificationStatus.Corrected;
    }

    public void MarkAsRejected()
    {
        if (VerificationStatus != VerificationStatus.Unverified)
            throw new InvalidOperationException("Field has already been verified");
        
        VerificationStatus = VerificationStatus.Rejected;
    }
}
