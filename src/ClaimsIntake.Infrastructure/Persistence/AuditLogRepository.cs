// =============================================
// Repository Implementation: AuditLogRepository
// Description: Audit log persistence with append-only enforcement
// Author: Infrastructure Team
// Date: February 2026
// =============================================

using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ClaimsIntake.Application.Interfaces;
using ClaimsIntake.Domain.Entities;

namespace ClaimsIntake.Infrastructure.Persistence;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly string _connectionString;

    public AuditLogRepository(string connectionString)
    {
        _connectionString = _connectionString;
    }

    public async Task AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO dbo.AuditLog (
                Timestamp, Actor, Action, EntityType, EntityId, Outcome, Details
            )
            VALUES (
                @Timestamp, @Actor, @Action, @EntityType, @EntityId, @Outcome, @Details
            )";

        using var connection = await CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(sql, new
        {
            auditEvent.Timestamp,
            auditEvent.Actor,
            auditEvent.Action,
            auditEvent.EntityType,
            auditEvent.EntityId,
            auditEvent.Outcome,
            auditEvent.Details
        });
    }

    public async Task<IEnumerable<AuditEvent>> GetByEntityAsync(
        string entityType, 
        string entityId, 
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT AuditId, Timestamp, Actor, Action, EntityType, EntityId, Outcome, Details
            FROM dbo.AuditLog
            WHERE EntityType = @EntityType AND EntityId = @EntityId
            ORDER BY Timestamp DESC";

        using var connection = await CreateConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<AuditEventRow>(sql, new { EntityType = entityType, EntityId = entityId });
        
        return rows.Select(MapToDomain);
    }

    private async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static AuditEvent MapToDomain(AuditEventRow row)
    {
        var auditEvent = (AuditEvent)Activator.CreateInstance(typeof(AuditEvent), nonPublic: true)!;
        
        typeof(AuditEvent).GetProperty(nameof(AuditEvent.AuditId))!.SetValue(auditEvent, row.AuditId);
        typeof(AuditEvent).GetProperty(nameof(AuditEvent.Timestamp))!.SetValue(auditEvent, row.Timestamp);
        typeof(AuditEvent).GetProperty(nameof(AuditEvent.Actor))!.SetValue(auditEvent, row.Actor);
        typeof(AuditEvent).GetProperty(nameof(AuditEvent.Action))!.SetValue(auditEvent, row.Action);
        typeof(AuditEvent).GetProperty(nameof(AuditEvent.EntityType))!.SetValue(auditEvent, row.EntityType);
        typeof(AuditEvent).GetProperty(nameof(AuditEvent.EntityId))!.SetValue(auditEvent, row.EntityId);
        typeof(AuditEvent).GetProperty(nameof(AuditEvent.Outcome))!.SetValue(auditEvent, row.Outcome);
        typeof(AuditEvent).GetProperty(nameof(AuditEvent.Details))!.SetValue(auditEvent, row.Details);
        
        return auditEvent;
    }

    private class AuditEventRow
    {
        public long AuditId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Actor { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Outcome { get; set; } = string.Empty;
        public string? Details { get; set; }
    }
}
