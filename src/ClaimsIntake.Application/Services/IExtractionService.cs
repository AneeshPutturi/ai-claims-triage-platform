// =============================================
// Service Interface: IExtractionService
// Description: AI-assisted extraction of claim data from documents
// Author: Application Team
// Date: February 2026
// =============================================

namespace ClaimsIntake.Application.Services;

/// <summary>
/// Service for extracting structured data from claim documents using AI.
/// All extraction results are marked as unverified by default.
/// </summary>
public interface IExtractionService
{
    /// <summary>
    /// Extracts structured data from a document.
    /// Returns extraction results with confidence scores.
    /// All results are marked as unverified.
    /// </summary>
    Task<ExtractionResult> ExtractFromDocumentAsync(
        Guid claimId,
        Guid documentId,
        string documentContent,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of AI extraction operation.
/// </summary>
public class ExtractionResult
{
    public List<ExtractedFieldData> Fields { get; set; } = new();
    public string ModelName { get; set; } = string.Empty;
    public string SystemPromptVersion { get; set; } = string.Empty;
    public string UserPromptVersion { get; set; } = string.Empty;
    public string SchemaVersion { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
    public DateTime ExtractedAt { get; set; }
}

/// <summary>
/// Individual extracted field with confidence score.
/// </summary>
public class ExtractedFieldData
{
    public string FieldName { get; set; } = string.Empty;
    public string? FieldValue { get; set; }
    public decimal ConfidenceScore { get; set; }
}
