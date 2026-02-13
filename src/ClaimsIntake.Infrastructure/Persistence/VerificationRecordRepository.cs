// =============================================
// Repository Implementation: VerificationRecordRepository
// Description: SQL persistence for verification records using Dapper
// Author: Infrastructure Team
// Date: February 2026
// =============================================

using System.Data;
using Dapper;
using ClaimsIntake.Application.Interfaces;
using ClaimsIntake.Domain.Entities;
using Microsoft.Data.SqlClient;

namespace ClaimsIntake.Infrastructure.Persistence;

/// <summary>
/// Repository for VerificationRecords table using explicit SQL.
/// Verification records are immutable - no update operations.
/// </summary>
public class VerificationRecordRepository : IVerificationRecordRepository
{
    private readonly string _connectionString;

    public VerificationRecordRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Guid> InsertAsync(VerificationRecord record, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO VerificationRecords (
                VerificationId,
                ClaimId,
                ExtractedFieldId,
                VerifiedBy,
                VerifiedAt,
                ActionTaken,
                CorrectedValue,
                VerificationNotes
            )
            VALUES (
                @VerificationId,
                @ClaimId,
                @ExtractedFieldId,
                @VerifiedBy,
                @VerifiedAt,
                @ActionTaken,
                @CorrectedValue,
                @VerificationNotes
            )";

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, new
        {
            record.VerificationId,
            record.ClaimId,
            record.ExtractedFieldId,
            record.VerifiedBy,
            record.VerifiedAt,
            record.ActionTaken,
            record.CorrectedValue,
            record.VerificationNotes
        });

        return record.VerificationId;
    }

    public async Task<IEnumerable<VerificationRecord>> GetByClaimIdAsync(
        Guid claimId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                VerificationId,
                ClaimId,
                ExtractedFieldId,
                VerifiedBy,
                VerifiedAt,
                ActionTaken,
                CorrectedValue,
                VerificationNotes
            FROM VerificationRecords
            WHERE ClaimId = @ClaimId
            ORDER BY VerifiedAt DESC";

        using var connection = new SqlConnection(_connectionString);
        var results = await connection.QueryAsync<VerificationRecordDto>(sql, new { ClaimId = claimId });
        
        return results.Select(MapToEntity);
    }

    public async Task<VerificationRecord?> GetByExtractedFieldIdAsync(
        Guid extractedFieldId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                VerificationId,
                ClaimId,
                ExtractedFieldId,
                VerifiedBy,
                VerifiedAt,
                ActionTaken,
                CorrectedValue,
                VerificationNotes
            FROM VerificationRecords
            WHERE ExtractedFieldId = @ExtractedFieldId";

        using var connection = new SqlConnection(_connectionString);
        var result = await connection.QuerySingleOrDefaultAsync<VerificationRecordDto>(
            sql, 
            new { ExtractedFieldId = extractedFieldId });
        
        return result != null ? MapToEntity(result) : null;
    }

    public async Task<bool> ExistsForFieldAsync(
        Guid extractedFieldId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM VerificationRecords
            WHERE ExtractedFieldId = @ExtractedFieldId";

        using var connection = new SqlConnection(_connectionString);
        var count = await connection.ExecuteScalarAsync<int>(sql, new { ExtractedFieldId = extractedFieldId });
        
        return count > 0;
    }

    private VerificationRecord MapToEntity(VerificationRecordDto dto)
    {
        return VerificationRecord.Create(
            dto.ClaimId,
            dto.ExtractedFieldId,
            dto.VerifiedBy,
            dto.ActionTaken,
            dto.CorrectedValue,
            dto.VerificationNotes);
    }

    private class VerificationRecordDto
    {
        public Guid VerificationId { get; set; }
        public Guid ClaimId { get; set; }
        public Guid ExtractedFieldId { get; set; }
        public string VerifiedBy { get; set; } = string.Empty;
        public DateTime VerifiedAt { get; set; }
        public string ActionTaken { get; set; } = string.Empty;
        public string? CorrectedValue { get; set; }
        public string? VerificationNotes { get; set; }
    }
}
