// =============================================
// Domain Entity: AuditEvent
// Description: Immutable audit log entry
// Author: Domain Team
// Date: February 2026
// =============================================

namespace ClaimsIntake.Domain.Entities;

/// <summary>
/// Immutable audit log entry.
/// Records every meaningful action for regulatory compliance.
/// </summary>
public class AuditEvent
{
    public long AuditId { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string Actor { get; private set; }
    public string Action { get; private set; }
    public string EntityType { get; private set; }
    public string EntityId { get; private set; }
    public string Outcome { get; private set; }
    public string? Details { get; private set; }

    private AuditEvent() { }

    public static AuditEvent Create(
        string actor,
        string action,
        string entityType,
        string entityId,
        string outcome,
        string? details = null)
    {
        if (string.IsNullOrWhiteSpace(actor))
            throw new ArgumentException("Actor is required", nameof(actor));

        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action is required", nameof(action));

        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required", nameof(entityType));

        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("Entity ID is required", nameof(entityId));

        if (string.IsNullOrWhiteSpace(outcome))
            throw new ArgumentException("Outcome is required", nameof(outcome));

        return new AuditEvent
        {
            Timestamp = DateTime.UtcNow,
            Actor = actor,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Outcome = outcome,
            Details = details
        };
    }
}
