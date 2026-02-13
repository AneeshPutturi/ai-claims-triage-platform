// =============================================
// Service Implementation: BlobStorageService
// Description: Azure Blob Storage wrapper with managed identity
// Author: Infrastructure Team
// Date: February 2026
// =============================================

using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ClaimsIntake.Application.Services;

namespace ClaimsIntake.Infrastructure.Services;

/// <summary>
/// Blob storage service using Azure Blob Storage with managed identity.
/// No Blob SDK calls outside this service.
/// </summary>
public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public BlobStorageService(string storageAccountUrl, string containerName)
    {
        // Use managed identity for authentication
        _blobServiceClient = new BlobServiceClient(
            new Uri(storageAccountUrl),
            new DefaultAzureCredential());
        
        _containerName = containerName;
    }

    public async Task<string> UploadDocumentAsync(
        Guid claimId,
        Guid documentId,
        string fileName,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        // Get container client
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

        // Deterministic blob naming: {ClaimId}/{DocumentId}.{extension}
        var extension = Path.GetExtension(fileName);
        var blobName = $"{claimId}/{documentId}{extension}";

        // Get blob client
        var blobClient = containerClient.GetBlobClient(blobName);

        // Upload options
        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            },
            // Prevent overwrite - fail if blob already exists
            Conditions = new BlobRequestConditions
            {
                IfNoneMatch = new Azure.ETag("*")
            },
            // Add metadata for tracking
            Metadata = new Dictionary<string, string>
            {
                ["ClaimId"] = claimId.ToString(),
                ["DocumentId"] = documentId.ToString(),
                ["OriginalFileName"] = fileName,
                ["UploadedAt"] = DateTime.UtcNow.ToString("o")
            }
        };

        // Upload blob (server-side streaming, no buffering in memory)
        await blobClient.UploadAsync(fileStream, uploadOptions, cancellationToken);

        // Return blob URI
        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadDocumentAsync(
        string blobUri,
        CancellationToken cancellationToken = default)
    {
        var blobClient = new BlobClient(new Uri(blobUri), new DefaultAzureCredential());

        // Download blob content as stream
        var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
        
        return response.Value.Content;
    }

    public async Task<bool> ExistsAsync(
        string blobUri,
        CancellationToken cancellationToken = default)
    {
        var blobClient = new BlobClient(new Uri(blobUri), new DefaultAzureCredential());
        return await blobClient.ExistsAsync(cancellationToken);
    }
}
