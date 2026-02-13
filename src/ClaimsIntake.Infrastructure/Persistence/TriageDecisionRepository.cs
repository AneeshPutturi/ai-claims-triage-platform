// =============================================
// Repository Implementation: TriageDecisionRepository
// Description: SQL persistence for triage decisions using Dapper
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
/// Repository for TriageDecision table using explicit SQL.
/// Triage decisions are immutable - no update operations.
/// </summary>
public class TriageDecisionRepository : ITriageDecisionRepository
{
    private readonly string _connectionString;

    public TriageDecisionRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Guid> InsertAsync(TriageDecision decision, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO TriageDecision (
                TriageDecisionId,
                ClaimId,
                RiskAssessmentId,
                Queue,
                RoutedAt,
                IsOverride,
                OverrideBy,
                OverrideReason
            )
            VALUES (
                @TriageDecisionId,
                @ClaimId,
                @RiskAssessmentId,
                @Queue,
                @RoutedAt,
                @IsOverride,
                @OverrideBy,
                @OverrideReason
            )";

        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, new
        {
            decision.TriageDecisionId,
            decision.ClaimId,
            decision.RiskAssessmentId,
            decision.Queue,
            decision.RoutedAt,
            decision.IsOverride,
            decision.OverrideBy,
            decision.OverrideReason
        });

        return decision.TriageDecisionId;
    }

    public async Task<TriageDecision?> GetLatestByClaimIdAsync(
        Guid claimId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT TOP 1
                TriageDecisionId,
                ClaimId,
                RiskAssessmentId,
                Queue,
                RoutedAt,
                IsOverride,
                OverrideBy,
                OverrideReason
            FROM TriageDecision
            WHERE ClaimId = @ClaimId
            ORDER BY RoutedAt DESC";

        using var connection = new SqlConnection(_connectionString);
        var result = await connection.QuerySingleOrDefaultAsync<TriageDecisionDto>(
            sql, 
            new { ClaimId = claimId });
        
        return result != null ? MapToEntity(result) : null;
    }

    public async Task<IEnumerable<TriageDecision>> GetAllByClaimIdAsync(
        Guid claimId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                TriageDecisionId,
                ClaimId,
                RiskAssessmentId,
                Queue,
                RoutedAt,
                IsOverride,
                OverrideBy,
                OverrideReason
            FROM TriageDecision
            WHERE ClaimId = @ClaimId
            ORDER BY RoutedAt DESC";

        using var connection = new SqlConnection(_connectionString);
        var results = await connection.QueryAsync<TriageDecisionDto>(sql, new { ClaimId = claimId });
        
        return results.Select(MapToEntity);
    }

    public async Task<IEnumerable<TriageDecision>> GetByQueueAsync(
        string queue,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                td.TriageDecisionId,
                td.ClaimId,
                td.RiskAssessmentId,
                td.Queue,
                td.RoutedAt,
                td.IsOverride,
                td.OverrideBy,
                td.OverrideReason
            FROM TriageDecision td
            INNER JOIN (
                SELECT ClaimId, MAX(RoutedAt) AS LatestRoutedAt
                FROM TriageDecision
                GROUP BY ClaimId
            ) latest ON td.ClaimId = latest.ClaimId AND td.RoutedAt = latest.LatestRoutedAt
            WHERE td.Queue = @Queue
            ORDER BY td.RoutedAt DESC";

        using var connection = new SqlConnection(_connectionString);
        var results = await connection.QueryAsync<TriageDecisionDto>(sql, new { Queue = queue });
        
        return results.Select(MapToEntity);
    }

    public async Task<bool> ExistsForRiskAssessmentAsync(
        Guid claimId,
        Guid riskAssessmentId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM TriageDecision
            WHERE ClaimId = @ClaimId AND RiskAssessmentId = @RiskAssessmentId";

        using var connection = new SqlConnection(_connectionString);
        var count = await connection.ExecuteScalarAsync<int>(sql, new 
        { 
            ClaimId = claimId,
            RiskAssessmentId = riskAssessmentId 
        });
        
        return count > 0;
    }

    private TriageDecision MapToEntity(TriageDecisionDto dto)
    {
        if (dto.IsOverride)
        {
            return TriageDecision.CreateOverride(
                dto.ClaimId,
                dto.RiskAssessmentId,
                dto.Queue,
                dto.OverrideBy!,
                dto.OverrideReason!);
        }
        
        return TriageDecision.Create(
            dto.ClaimId,
            dto.RiskAssessmentId,
            dto.Queue);
    }

    private class TriageDecisionDto
    {
        public Guid TriageDecisionId { get; set; }
        public Guid ClaimId { get; set; }
        public Guid RiskAssessmentId { get; set; }
        public string Queue { get; set; } = string.Empty;
        public DateTime RoutedAt { get; set; }
        public bool IsOverride { get; set; }
        public string? OverrideBy { get; set; }
        public string? OverrideReason { get; set; }
    }
}
