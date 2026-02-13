// =============================================
// Repository Implementation: ExtractedFieldRepository
// Description: SQL persistence for extracted fields using Dapper
// Author: Infrastructure Team
// Date: February 2026
// =============================================

using System.Data;
using Dapper;
using ClaimsIntake.Application.Interfaces;
using ClaimsIntake.Domain.Entities;
using ClaimsIntake.Domain.Enums;
using Microsoft.Data.SqlClient;

namespace ClaimsIntake.Infrastructure.Persistence;

/// <summary>
/// Repository for ExtractedFields table using explicit SQL.
/// No ORM magic - every query is visible and auditable.
/// </summary>
public class ExtractedFieldRepository : IExtractedFieldRepository
{
    private readonly string _connectionString;

    public ExtractedFieldRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Guid> InsertAsync(ExtractedField field, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO ExtractedFields (
                ExtractedFieldId,
                ClaimId,
                DocumentId,
                FieldName,
                FieldValue,
                ConfidenceScore,
                VerificationStatus,
                ExtractedAt,
                ExtractedByModel,
                SystemPromptVersion,
                UserPromptVersion,
                SchemaVersion
            )
            VALUES (
                @ExtractedFieldId,
                @ClaimId,
                @DocumentId,
                @FieldName,
                @FieldValue,
                @ConfidenceScore,
                @VerificationStatus,
                @ExtractedAt,
                @ExtractedByModel,
                @SystemPromptVersion,
                @UserPromptVersion,
                @SchemaVersion
            )";

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, new
        {
            field.ExtractedFieldId,
            field.ClaimId,
            field.DocumentId,
            field.FieldName,
            field.FieldValue,
            field.ConfidenceScore,
            VerificationStatus = field.VerificationStatus.ToString(),
            field.ExtractedAt,
            field.ExtractedByModel,
            field.SystemPromptVersion,
            field.UserPromptVersion,
            field.SchemaVersion
        });

        return field.ExtractedFieldId;
    }

    public async Task<IEnumerable<ExtractedField>> GetByClaimIdAsync(
        Guid claimId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                ExtractedFieldId,
                ClaimId,
                DocumentId,
                FieldName,
                FieldValue,
                ConfidenceScore,
                VerificationStatus,
                ExtractedAt,
                ExtractedByModel,
                SystemPromptVersion,
                UserPromptVersion,
                SchemaVersion
            FROM ExtractedFields
            WHERE ClaimId = @ClaimId
            ORDER BY ExtractedAt DESC";

        using var connection = new SqlConnection(_connectionString);
        var results = await connection.QueryAsync<ExtractedFieldDto>(sql, new { ClaimId = claimId });
        
        return results.Select(MapToEntity);
    }

    public async Task<IEnumerable<ExtractedField>> GetByDocumentIdAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                ExtractedFieldId,
                ClaimId,
                DocumentId,
                FieldName,
                FieldValue,
                ConfidenceScore,
                VerificationStatus,
                ExtractedAt,
                ExtractedByModel,
                SystemPromptVersion,
                UserPromptVersion,
                SchemaVersion
            FROM ExtractedFields
            WHERE DocumentId = @DocumentId
            ORDER BY ExtractedAt DESC";

        using var connection = new SqlConnection(_connectionString);
        var results = await connection.QueryAsync<ExtractedFieldDto>(sql, new { DocumentId = documentId });
        
        return results.Select(MapToEntity);
    }

    public async Task<ExtractedField?> GetByIdAsync(
        Guid extractedFieldId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                ExtractedFieldId,
                ClaimId,
                DocumentId,
                FieldName,
                FieldValue,
                ConfidenceScore,
                VerificationStatus,
                ExtractedAt,
                ExtractedByModel,
                SystemPromptVersion,
                UserPromptVersion,
                SchemaVersion
            FROM ExtractedFields
            WHERE ExtractedFieldId = @ExtractedFieldId";

        using var connection = new SqlConnection(_connectionString);
        var result = await connection.QuerySingleOrDefaultAsync<ExtractedFieldDto>(
            sql, 
            new { ExtractedFieldId = extractedFieldId });
        
        return result != null ? MapToEntity(result) : null;
    }

    public async Task UpdateVerificationStatusAsync(
        Guid extractedFieldId,
        string verificationStatus,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE ExtractedFields
            SET VerificationStatus = @VerificationStatus
            WHERE ExtractedFieldId = @ExtractedFieldId";

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, new
        {
            ExtractedFieldId = extractedFieldId,
            VerificationStatus = verificationStatus
        });
    }

    private ExtractedField MapToEntity(ExtractedFieldDto dto)
    {
        return ExtractedField.Create(
            dto.ClaimId,
            dto.DocumentId,
            dto.FieldName,
            dto.FieldValue,
            dto.ConfidenceScore,
            dto.ExtractedByModel,
            dto.SystemPromptVersion,
            dto.UserPromptVersion,
            dto.SchemaVersion);
    }

    private class ExtractedFieldDto
    {
        public Guid ExtractedFieldId { get; set; }
        public Guid ClaimId { get; set; }
        public Guid DocumentId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? FieldValue { get; set; }
        public decimal ConfidenceScore { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        public DateTime ExtractedAt { get; set; }
        public string ExtractedByModel { get; set; } = string.Empty;
        public string SystemPromptVersion { get; set; } = string.Empty;
        public string UserPromptVersion { get; set; } = string.Empty;
        public string SchemaVersion { get; set; } = string.Empty;
    }
}
