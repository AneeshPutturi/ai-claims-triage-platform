// =============================================
// Domain Enum: ClaimStatus
// Description: Lifecycle states for a claim
// Author: Domain Team
// Date: February 2026
// =============================================
// Purpose: Represents the explicit lifecycle state of a claim.
// Values match database CHECK constraint in Claims table.
// State transitions are controlled by domain logic.
// =============================================

namespace ClaimsIntake.Domain.Enums;

/// <summary>
/// Lifecycle states for a claim.
/// Transitions must follow: Submitted → Validated → Verified → Triaged
/// </summary>
public enum ClaimStatus
{
    /// <summary>
    /// Initial state when claim is first received.
    /// System has accepted claim data but has not yet performed validation.
    /// </summary>
    Submitted = 0,

    /// <summary>
    /// Claim has passed automated validation checks.
    /// Policy number verified, loss date valid, all required fields present.
    /// Ready for human review.
    /// </summary>
    Validated = 1,

    /// <summary>
    /// Human adjuster has reviewed and confirmed claim data accuracy.
    /// AI-extracted information (if any) has been verified.
    /// Ready for risk assessment.
    /// </summary>
    Verified = 2,

    /// <summary>
    /// Claim has been assigned a risk level and routed to appropriate queue.
    /// Ready for assignment to adjuster or investigation team.
    /// </summary>
    Triaged = 3
}
