// =============================================
// Repository Implementation: ClaimDocumentRepository
// Description: Document metadata persistence using explicit SQL
// Author: Infrastructure Team
// Date: February 2026
// =============================================

using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ClaimsIntake.Application.Interfaces;
using ClaimsIntake.Domain.Entities;

namespace ClaimsIntake.Infrastructure.Persistence;

public class ClaimDocumentRepository : IClaimDocumentRepository
{
    private readonly string _connectionString;

    public ClaimDocumentRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task AddAsync(ClaimDocument document, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO dbo.ClaimDocuments (
                DocumentId, ClaimId, FileName, DocumentType, StorageLocation,
                FileSizeBytes, ContentType, UploadedBy, DocumentStatus
            )
            VALUES (
                @DocumentId, @ClaimId, @FileName, @DocumentType, @StorageLocation,
                @FileSizeBytes, @ContentType, @UploadedBy, @DocumentStatus
            )";

        using var connection = await CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(sql, new
        {
            document.DocumentId,
            document.ClaimId,
            document.FileName,
            document.DocumentType,
            document.StorageLocation,
            document.FileSizeBytes,
            document.ContentType,
            document.UploadedBy,
            document.DocumentStatus
        });
    }

    public async Task<ClaimDocument?> GetByIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT DocumentId, ClaimId, FileName, DocumentType, StorageLocation,
                   FileSizeBytes, ContentType, UploadedAt, UploadedBy, DocumentStatus
            FROM dbo.ClaimDocuments
            WHERE DocumentId = @DocumentId";

        using var connection = await CreateConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<ClaimDocumentRow>(sql, new { DocumentId = documentId });
        
        return row != null ? MapToDomain(row) : null;
    }

    public async Task<IEnumerable<ClaimDocument>> GetByClaimIdAsync(Guid claimId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT DocumentId, ClaimId, FileName, DocumentType, StorageLocation,
                   FileSizeBytes, ContentType, UploadedAt, UploadedBy, DocumentStatus
            FROM dbo.ClaimDocuments
            WHERE ClaimId = @ClaimId
            ORDER BY UploadedAt DESC";

        using var connection = await CreateConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<ClaimDocumentRow>(sql, new { ClaimId = claimId });
        
        return rows.Select(MapToDomain);
    }

    private async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static ClaimDocument MapToDomain(ClaimDocumentRow row)
    {
        var document = (ClaimDocument)Activator.CreateInstance(typeof(ClaimDocument), nonPublic: true)!;
        
        typeof(ClaimDocument).GetProperty(nameof(ClaimDocument.DocumentId))!.SetValue(document, row.DocumentId);
        typeof(ClaimDocument).GetProperty(nameof(ClaimDocument.ClaimId))!.SetValue(document, row.ClaimId);
        typeof(ClaimDocument).GetProperty(nameof(ClaimDocument.FileName))!.SetValue(document, row.FileName);
        typeof(ClaimDocument).GetProperty(nameof(ClaimDocument.DocumentType))!.SetValue(document, row.DocumentType);
        typeof(ClaimDocument).GetProperty(nameof(ClaimDocument.StorageLocation))!.SetValue(document, row.StorageLocation);
        typeof(ClaimDocument).GetProperty(nameof(ClaimDocument.FileSizeBytes))!.SetValue(document, row.FileSizeBytes);
        typeof(ClaimDocument).GetProperty(nameof(ClaimDocument.ContentType))!.SetValue(document, row.ContentType);
        typeof(ClaimDocument).GetProperty(nameof(ClaimDocument.UploadedAt))!.SetValue(document, row.UploadedAt);
        typeof(ClaimDocument).GetProperty(nameof(ClaimDocument.UploadedBy))!.SetValue(document, row.UploadedBy);
        typeof(ClaimDocument).GetProperty(nameof(ClaimDocument.DocumentStatus))!.SetValue(document, row.DocumentStatus);
        
        return document;
    }

    private class ClaimDocumentRow
    {
        public Guid DocumentId { get; set; }
        public Guid ClaimId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string StorageLocation { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
        public string DocumentStatus { get; set; } = string.Empty;
    }
}
