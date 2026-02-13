// =============================================
// Repository Implementation: PolicySnapshotRepository
// Description: Policy snapshot persistence using explicit SQL
// Author: Infrastructure Team
// Date: February 2026
// =============================================

using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ClaimsIntake.Application.Interfaces;
using ClaimsIntake.Domain.Entities;
using ClaimsIntake.Domain.Enums;
using ClaimsIntake.Domain.ValueObjects;

namespace ClaimsIntake.Infrastructure.Persistence;

public class PolicySnapshotRepository : IPolicySnapshotRepository
{
    private readonly string _connectionString;

    public PolicySnapshotRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task AddAsync(PolicySnapshot snapshot, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO dbo.PolicySnapshot (
                SnapshotId, ClaimId, PolicyId, EffectiveDate, ExpirationDate,
                CoverageStatus, CoveredLossTypes, CoverageLimits, Deductibles, SnapshotCreatedAt
            )
            VALUES (
                @SnapshotId, @ClaimId, @PolicyId, @EffectiveDate, @ExpirationDate,
                @CoverageStatus, @CoveredLossTypes, @CoverageLimits, @Deductibles, @SnapshotCreatedAt
            )";

        using var connection = await CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(sql, new
        {
            snapshot.SnapshotId,
            snapshot.ClaimId,
            PolicyId = snapshot.PolicyId.Value,
            snapshot.EffectiveDate,
            snapshot.ExpirationDate,
            CoverageStatus = snapshot.CoverageStatus.ToString(),
            snapshot.CoveredLossTypes,
            snapshot.CoverageLimits,
            snapshot.Deductibles,
            snapshot.SnapshotCreatedAt
        });
    }

    public async Task<PolicySnapshot?> GetByClaimIdAsync(Guid claimId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT SnapshotId, ClaimId, PolicyId, EffectiveDate, ExpirationDate,
                   CoverageStatus, CoveredLossTypes, CoverageLimits, Deductibles, SnapshotCreatedAt
            FROM dbo.PolicySnapshot
            WHERE ClaimId = @ClaimId";

        using var connection = await CreateConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<PolicySnapshotRow>(sql, new { ClaimId = claimId });
        
        return row != null ? MapToDomain(row) : null;
    }

    private async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static PolicySnapshot MapToDomain(PolicySnapshotRow row)
    {
        var snapshot = (PolicySnapshot)Activator.CreateInstance(typeof(PolicySnapshot), nonPublic: true)!;
        
        typeof(PolicySnapshot).GetProperty(nameof(PolicySnapshot.SnapshotId))!.SetValue(snapshot, row.SnapshotId);
        typeof(PolicySnapshot).GetProperty(nameof(PolicySnapshot.ClaimId))!.SetValue(snapshot, row.ClaimId);
        typeof(PolicySnapshot).GetProperty(nameof(PolicySnapshot.PolicyId))!.SetValue(snapshot, PolicyId.From(row.PolicyId));
        typeof(PolicySnapshot).GetProperty(nameof(PolicySnapshot.EffectiveDate))!.SetValue(snapshot, row.EffectiveDate);
        typeof(PolicySnapshot).GetProperty(nameof(PolicySnapshot.ExpirationDate))!.SetValue(snapshot, row.ExpirationDate);
        typeof(PolicySnapshot).GetProperty(nameof(PolicySnapshot.CoverageStatus))!.SetValue(snapshot, Enum.Parse<CoverageStatus>(row.CoverageStatus));
        typeof(PolicySnapshot).GetProperty(nameof(PolicySnapshot.CoveredLossTypes))!.SetValue(snapshot, row.CoveredLossTypes);
        typeof(PolicySnapshot).GetProperty(nameof(PolicySnapshot.CoverageLimits))!.SetValue(snapshot, row.CoverageLimits);
        typeof(PolicySnapshot).GetProperty(nameof(PolicySnapshot.Deductibles))!.SetValue(snapshot, row.Deductibles);
        typeof(PolicySnapshot).GetProperty(nameof(PolicySnapshot.SnapshotCreatedAt))!.SetValue(snapshot, row.SnapshotCreatedAt);
        
        return snapshot;
    }

    private class PolicySnapshotRow
    {
        public Guid SnapshotId { get; set; }
        public Guid ClaimId { get; set; }
        public string PolicyId { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string CoverageStatus { get; set; } = string.Empty;
        public string CoveredLossTypes { get; set; } = string.Empty;
        public string? CoverageLimits { get; set; }
        public string? Deductibles { get; set; }
        public DateTime SnapshotCreatedAt { get; set; }
    }
}
