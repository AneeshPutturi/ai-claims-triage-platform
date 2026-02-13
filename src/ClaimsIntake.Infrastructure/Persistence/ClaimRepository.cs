// =============================================
// Repository Implementation: ClaimRepository
// Description: Claim persistence using explicit SQL with Dapper
// Author: Infrastructure Team
// Date: February 2026
// =============================================
// Purpose: Explicit SQL queries, no ORM magic.
// Uses managed identity for SQL connection.
// =============================================

using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ClaimsIntake.Application.Interfaces;
using ClaimsIntake.Domain.Entities;
using ClaimsIntake.Domain.Enums;
using ClaimsIntake.Domain.ValueObjects;

namespace ClaimsIntake.Infrastructure.Persistence;

public class ClaimRepository : IClaimRepository
{
    private readonly string _connectionString;

    public ClaimRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Claim?> GetByIdAsync(Guid claimId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT ClaimId, ClaimNumber, PolicyNumber, LossDate, LossType, LossLocation,
                   LossDescription, Status, CreatedAt, UpdatedAt, SubmittedBy, RowVersion
            FROM dbo.Claims
            WHERE ClaimId = @ClaimId";

        using var connection = await CreateConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<ClaimRow>(sql, new { ClaimId = claimId });
        
        return row != null ? MapToDomain(row) : null;
    }

    public async Task<Claim?> GetByClaimNumberAsync(ClaimNumber claimNumber, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT ClaimId, ClaimNumber, PolicyNumber, LossDate, LossType, LossLocation,
                   LossDescription, Status, CreatedAt, UpdatedAt, SubmittedBy, RowVersion
            FROM dbo.Claims
            WHERE ClaimNumber = @ClaimNumber";

        using var connection = await CreateConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<ClaimRow>(sql, new { ClaimNumber = claimNumber.Value });
        
        return row != null ? MapToDomain(row) : null;
    }

    public async Task AddAsync(Claim claim, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO dbo.Claims (
                ClaimId, ClaimNumber, PolicyNumber, LossDate, LossType, LossLocation,
                LossDescription, Status, CreatedAt, UpdatedAt, SubmittedBy
            )
            VALUES (
                @ClaimId, @ClaimNumber, @PolicyNumber, @LossDate, @LossType, @LossLocation,
                @LossDescription, @Status, @CreatedAt, @UpdatedAt, @SubmittedBy
            )";

        using var connection = await CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(sql, new
        {
            claim.ClaimId,
            ClaimNumber = claim.ClaimNumber.Value,
            PolicyNumber = claim.PolicyNumber.Value,
            LossDate = claim.LossDate.Value,
            claim.LossType,
            claim.LossLocation,
            claim.LossDescription,
            Status = claim.Status.ToString(),
            claim.CreatedAt,
            claim.UpdatedAt,
            claim.SubmittedBy
        });
    }

    public async Task UpdateAsync(Claim claim, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE dbo.Claims
            SET Status = @Status,
                LossDescription = @LossDescription,
                UpdatedAt = @UpdatedAt
            WHERE ClaimId = @ClaimId AND RowVersion = @RowVersion";

        using var connection = await CreateConnectionAsync(cancellationToken);
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            claim.ClaimId,
            Status = claim.Status.ToString(),
            claim.LossDescription,
            claim.UpdatedAt,
            claim.RowVersion
        });

        if (rowsAffected == 0)
            throw new InvalidOperationException(
                $"Concurrency conflict detected for claim {claim.ClaimNumber}. " +
                "The claim was modified by another process.");
    }

    public async Task<int> GetNextSequenceNumberAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT ISNULL(MAX(CAST(RIGHT(ClaimNumber, 6) AS INT)), 0) + 1
            FROM dbo.Claims
            WHERE ClaimNumber LIKE @YearPrefix";

        var year = DateTime.UtcNow.Year;
        var yearPrefix = $"{year}-%";

        using var connection = await CreateConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<int>(sql, new { YearPrefix = yearPrefix });
    }

    public async Task UpdateStatusAsync(Guid claimId, string status, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE dbo.Claims
            SET Status = @Status,
                UpdatedAt = @UpdatedAt
            WHERE ClaimId = @ClaimId";

        using var connection = await CreateConnectionAsync(cancellationToken);
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            ClaimId = claimId,
            Status = status,
            UpdatedAt = DateTime.UtcNow
        });

        if (rowsAffected == 0)
            throw new InvalidOperationException($"Claim {claimId} not found");
    }

    private async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static Claim MapToDomain(ClaimRow row)
    {
        // Use reflection to create claim with private constructor
        // In production, consider using a factory method or making constructor internal
        var claim = (Claim)Activator.CreateInstance(typeof(Claim), nonPublic: true)!;
        
        var claimIdProperty = typeof(Claim).GetProperty(nameof(Claim.ClaimId))!;
        claimIdProperty.SetValue(claim, row.ClaimId);
        
        var claimNumberProperty = typeof(Claim).GetProperty(nameof(Claim.ClaimNumber))!;
        claimNumberProperty.SetValue(claim, ClaimNumber.From(row.ClaimNumber));
        
        var policyNumberProperty = typeof(Claim).GetProperty(nameof(Claim.PolicyNumber))!;
        policyNumberProperty.SetValue(claim, PolicyId.From(row.PolicyNumber));
        
        var lossDateProperty = typeof(Claim).GetProperty(nameof(Claim.LossDate))!;
        lossDateProperty.SetValue(claim, LossDate.From(row.LossDate));
        
        var lossTypeProperty = typeof(Claim).GetProperty(nameof(Claim.LossType))!;
        lossTypeProperty.SetValue(claim, row.LossType);
        
        var lossLocationProperty = typeof(Claim).GetProperty(nameof(Claim.LossLocation))!;
        lossLocationProperty.SetValue(claim, row.LossLocation);
        
        var lossDescriptionProperty = typeof(Claim).GetProperty(nameof(Claim.LossDescription))!;
        lossDescriptionProperty.SetValue(claim, row.LossDescription);
        
        var statusProperty = typeof(Claim).GetProperty(nameof(Claim.Status))!;
        statusProperty.SetValue(claim, Enum.Parse<ClaimStatus>(row.Status));
        
        var createdAtProperty = typeof(Claim).GetProperty(nameof(Claim.CreatedAt))!;
        createdAtProperty.SetValue(claim, row.CreatedAt);
        
        var updatedAtProperty = typeof(Claim).GetProperty(nameof(Claim.UpdatedAt))!;
        updatedAtProperty.SetValue(claim, row.UpdatedAt);
        
        var submittedByProperty = typeof(Claim).GetProperty(nameof(Claim.SubmittedBy))!;
        submittedByProperty.SetValue(claim, row.SubmittedBy);
        
        var rowVersionProperty = typeof(Claim).GetProperty(nameof(Claim.RowVersion))!;
        rowVersionProperty.SetValue(claim, row.RowVersion);
        
        return claim;
    }

    private class ClaimRow
    {
        public Guid ClaimId { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public DateTime LossDate { get; set; }
        public string LossType { get; set; } = string.Empty;
        public string LossLocation { get; set; } = string.Empty;
        public string LossDescription { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string SubmittedBy { get; set; } = string.Empty;
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
