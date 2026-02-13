// =============================================
// Domain Entity: TriageDecision
// Description: Routing decision snapshot for a claim
// Author: Domain Team
// Date: February 2026
// =============================================

namespace ClaimsIntake.Domain.Entities;

/// <summary>
/// Represents a triage routing decision for a claim.
/// Routing is operational, not legal. Routing prioritizes human effort.
/// Immutable once created - routing decisions are historical snapshots.
/// </summary>
public class TriageDecision
{
    public Guid TriageDecisionId { get; private set; }
    public Guid ClaimId { get; private set; }
    public Guid RiskAssessmentId { get; private set; }
    public string Queue { get; private set; }
    public DateTime RoutedAt { get; private set; }
    public bool IsOverride { get; private set; }
    public string? OverrideBy { get; private set; }
    public string? OverrideReason { get; private set; }

    private TriageDecision() { }

    public static TriageDecision Create(
        Guid claimId,
        Guid riskAssessmentId,
        string queue)
    {
        if (claimId == Guid.Empty)
            throw new ArgumentException("ClaimId cannot be empty", nameof(claimId));
        
        if (riskAssessmentId == Guid.Empty)
            throw new ArgumentException("RiskAssessmentId cannot be empty", nameof(riskAssessmentId));
        
        if (string.IsNullOrWhiteSpace(queue))
            throw new ArgumentException("Queue cannot be empty", nameof(queue));
        
        // Validate queue name
        var validQueues = new[] { "Auto-Review", "Standard Review", "Manual Investigation" };
        if (!validQueues.Contains(queue))
            throw new ArgumentException(
                $"Queue must be one of: {string.Join(", ", validQueues)}", 
                nameof(queue));

        return new TriageDecision
        {
            TriageDecisionId = Guid.NewGuid(),
            ClaimId = claimId,
            RiskAssessmentId = riskAssessmentId,
            Queue = queue,
            RoutedAt = DateTime.UtcNow,
            IsOverride = false,
            OverrideBy = null,
            OverrideReason = null
        };
    }

    public static TriageDecision CreateOverride(
        Guid claimId,
        Guid riskAssessmentId,
        string queue,
        string overrideBy,
        string overrideReason)
    {
        if (string.IsNullOrWhiteSpace(overrideBy))
            throw new ArgumentException("OverrideBy cannot be empty for override", nameof(overrideBy));
        
        if (string.IsNullOrWhiteSpace(overrideReason))
            throw new ArgumentException("OverrideReason cannot be empty for override", nameof(overrideReason));

        var decision = Create(claimId, riskAssessmentId, queue);
        
        return new TriageDecision
        {
            TriageDecisionId = decision.TriageDecisionId,
            ClaimId = decision.ClaimId,
            RiskAssessmentId = decision.RiskAssessmentId,
            Queue = decision.Queue,
            RoutedAt = decision.RoutedAt,
            IsOverride = true,
            OverrideBy = overrideBy,
            OverrideReason = overrideReason
        };
    }
}
