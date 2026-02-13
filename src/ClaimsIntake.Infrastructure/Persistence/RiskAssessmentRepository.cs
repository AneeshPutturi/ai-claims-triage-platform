// =============================================
// Repository Implementation: RiskAssessmentRepository
// Description: SQL persistence for risk assessments using Dapper
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
/// Repository for RiskAssessment table using explicit SQL.
/// Risk assessments are immutable - no update operations.
/// </summary>
public class RiskAssessmentRepository : IRiskAssessmentRepository
{
    private readonly string _connectionString;

    public RiskAssessmentRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Guid> InsertAsync(RiskAssessment assessment, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO RiskAssessment (
                RiskAssessmentId,
                ClaimId,
                RiskLevel,
                RuleSignals,
                AISignals,
                OverallScore,
                CreatedAt,
                AssessedByModel
            )
            VALUES (
                @RiskAssessmentId,
                @ClaimId,
                @RiskLevel,
                @RuleSignals,
                @AISignals,
                @OverallScore,
                @CreatedAt,
                @AssessedByModel
            )";

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, new
        {
            assessment.RiskAssessmentId,
            assessment.ClaimId,
            RiskLevel = assessment.RiskLevel.ToString(),
            assessment.RuleSignals,
            assessment.AISignals,
            assessment.OverallScore,
            assessment.CreatedAt,
            assessment.AssessedByModel
        });

        return assessment.RiskAssessmentId;
    }

    public async Task<RiskAssessment?> GetLatestByClaimIdAsync(
        Guid claimId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT TOP 1
                RiskAssessmentId,
                ClaimId,
                RiskLevel,
                RuleSignals,
                AISignals,
                OverallScore,
                CreatedAt,
                AssessedByModel
            FROM RiskAssessment
            WHERE ClaimId = @ClaimId
            ORDER BY CreatedAt DESC";

        using var connection = new SqlConnection(_connectionString);
        var result = await connection.QuerySingleOrDefaultAsync<RiskAssessmentDto>(
            sql, 
            new { ClaimId = claimId });
        
        return result != null ? MapToEntity(result) : null;
    }

    public async Task<IEnumerable<RiskAssessment>> GetAllByClaimIdAsync(
        Guid claimId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                RiskAssessmentId,
                ClaimId,
                RiskLevel,
                RuleSignals,
                AISignals,
                OverallScore,
                CreatedAt,
                AssessedByModel
            FROM RiskAssessment
            WHERE ClaimId = @ClaimId
            ORDER BY CreatedAt DESC";

        using var connection = new SqlConnection(_connectionString);
        var results = await connection.QueryAsync<RiskAssessmentDto>(sql, new { ClaimId = claimId });
        
        return results.Select(MapToEntity);
    }

    public async Task<RiskAssessment?> GetByIdAsync(
        Guid riskAssessmentId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                RiskAssessmentId,
                ClaimId,
                RiskLevel,
                RuleSignals,
                AISignals,
                OverallScore,
                CreatedAt,
                AssessedByModel
            FROM RiskAssessment
            WHERE RiskAssessmentId = @RiskAssessmentId";

        using var connection = new SqlConnection(_connectionString);
        var result = await connection.QuerySingleOrDefaultAsync<RiskAssessmentDto>(
            sql, 
            new { RiskAssessmentId = riskAssessmentId });
        
        return result != null ? MapToEntity(result) : null;
    }

    private RiskAssessment MapToEntity(RiskAssessmentDto dto)
    {
        var riskLevel = Enum.Parse<RiskLevel>(dto.RiskLevel);
        
        return RiskAssessment.Create(
            dto.ClaimId,
            riskLevel,
            dto.RuleSignals,
            dto.AISignals,
            dto.OverallScore,
            dto.AssessedByModel);
    }

    private class RiskAssessmentDto
    {
        public Guid RiskAssessmentId { get; set; }
        public Guid ClaimId { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public string RuleSignals { get; set; } = string.Empty;
        public string AISignals { get; set; } = string.Empty;
        public decimal OverallScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AssessedByModel { get; set; }
    }
}
