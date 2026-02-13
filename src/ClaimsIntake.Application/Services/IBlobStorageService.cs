// =============================================
// Service Interface: IBlobStorageService
// Description: Blob storage operations abstraction
// Author: Application Team
// Date: February 2026
// =============================================

namespace ClaimsIntake.Application.Services;

/// <summary>
/// Service for blob storage operations.
/// Wraps Azure Blob Storage SDK behind interface.
/// </summary>
public interface IBlobStorageService
{
    /// <summary>
    /// Upload document content to blob storage.
    /// Returns blob URI on success.
    /// </summary>
    Task<string> UploadDocumentAsync(
        Guid claimId,
        Guid documentId,
        string fileName,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Download document content from blob storage.
    /// Returns stream for reading blob content.
    /// </summary>
    Task<Stream> DownloadDocumentAsync(
        string blobUri,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if blob exists
    /// </summary>
    Task<bool> ExistsAsync(
        string blobUri,
        CancellationToken cancellationToken = default);
}
