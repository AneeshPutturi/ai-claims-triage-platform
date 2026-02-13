// =============================================
// Repository Interface: IAuditLogRepository
// Description: Defines audit log persistence operations
// Author: Application Team
// Date: February 2026
// =============================================

using ClaimsIntake.Domain.Entities;

namespace ClaimsIntake.Application.Interfaces;

/// <summary>
/// Repository for audit log persistence.
/// Audit logs are append-only and immutable.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Add audit event to log
    /// Append-only operation - no updates or deletes
    /// </summary>
    Task AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve audit events for specific entity
    /// </summary>
    Task<IEnumerable<AuditEvent>> GetByEntityAsync(
        string entityType, 
        string entityId, 
        CancellationToken cancellationToken = default);
}
