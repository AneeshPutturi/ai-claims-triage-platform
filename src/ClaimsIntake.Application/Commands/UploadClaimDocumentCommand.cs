// =============================================
// Application Command: UploadClaimDocumentCommand
// Description: Command for uploading claim document
// Author: Application Team
// Date: February 2026
// =============================================

namespace ClaimsIntake.Application.Commands;

/// <summary>
/// Command for uploading a document to a claim.
/// Contains claim ID and document metadata, no blob or HTTP references.
/// </summary>
public record UploadClaimDocumentCommand(
    Guid ClaimId,
    string FileName,
    string DocumentType,
    string ContentType,
    long FileSizeBytes,
    Stream FileStream,
    string UploadedBy)
{
    /// <summary>
    /// Validate command data before processing
    /// </summary>
    public void Validate()
    {
        var errors = new List<string>();

        if (ClaimId == Guid.Empty)
            errors.Add("Claim ID is required");

        if (string.IsNullOrWhiteSpace(FileName))
            errors.Add("File name is required");

        if (string.IsNullOrWhiteSpace(DocumentType))
            errors.Add("Document type is required");

        if (string.IsNullOrWhiteSpace(ContentType))
            errors.Add("Content type is required");

        if (FileSizeBytes <= 0)
            errors.Add("File size must be positive");

        if (FileSizeBytes > 25 * 1024 * 1024) // 25 MB
            errors.Add($"Document size exceeds maximum allowed size of 25 MB. File size: {FileSizeBytes / (1024 * 1024)} MB");

        if (FileStream == null || !FileStream.CanRead)
            errors.Add("File stream is required and must be readable");

        if (string.IsNullOrWhiteSpace(UploadedBy))
            errors.Add("Uploader identity is required");

        // Validate file extension
        var extension = Path.GetExtension(FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
        if (!allowedExtensions.Contains(extension))
            errors.Add($"Unsupported file type: {extension}. Allowed types: PDF, JPEG, PNG");

        if (errors.Any())
            throw new ArgumentException($"Validation failed: {string.Join(", ", errors)}");
    }
}
