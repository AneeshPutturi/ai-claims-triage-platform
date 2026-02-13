// =============================================
// Domain Entity: ClaimDocument
// Description: Legal artifact metadata
// Author: Domain Team
// Date: February 2026
// =============================================

namespace ClaimsIntake.Domain.Entities;

/// <summary>
/// Metadata for a claim document (legal artifact).
/// File content stored in blob storage, metadata stored in database.
/// Immutable once created.
/// </summary>
public class ClaimDocument
{
    public Guid DocumentId { get; private set; }
    public Guid ClaimId { get; private set; }
    public string FileName { get; private set; }
    public string DocumentType { get; private set; }
    public string StorageLocation { get; private set; }
    public long FileSizeBytes { get; private set; }
    public string ContentType { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public string UploadedBy { get; private set; }
    public string DocumentStatus { get; private set; }

    private ClaimDocument() { }

    public static ClaimDocument Create(
        Guid claimId,
        string fileName,
        string documentType,
        string storageLocation,
        long fileSizeBytes,
        string contentType,
        string uploadedBy)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required", nameof(fileName));

        if (string.IsNullOrWhiteSpace(documentType))
            throw new ArgumentException("Document type is required", nameof(documentType));

        if (string.IsNullOrWhiteSpace(storageLocation))
            throw new ArgumentException("Storage location is required", nameof(storageLocation));

        if (fileSizeBytes <= 0)
            throw new ArgumentException("File size must be positive", nameof(fileSizeBytes));

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type is required", nameof(contentType));

        if (string.IsNullOrWhiteSpace(uploadedBy))
            throw new ArgumentException("Uploader identity is required", nameof(uploadedBy));

        return new ClaimDocument
        {
            DocumentId = Guid.NewGuid(),
            ClaimId = claimId,
            FileName = fileName,
            DocumentType = documentType,
            StorageLocation = storageLocation,
            FileSizeBytes = fileSizeBytes,
            ContentType = contentType,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = uploadedBy,
            DocumentStatus = "Active"
        };
    }

    /// <summary>
    /// Mark document as superseded by a newer version
    /// </summary>
    public void MarkAsSuperseded()
    {
        if (DocumentStatus != "Active")
            throw new InvalidOperationException("Only active documents can be superseded");

        DocumentStatus = "Superseded";
    }
}
