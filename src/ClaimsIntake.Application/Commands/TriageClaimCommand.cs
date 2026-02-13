// =============================================
// Command: TriageClaimCommand
// Description: Route claim to appropriate processing queue
// Author: Application Team
// Date: February 2026
// =============================================

namespace ClaimsIntake.Application.Commands;

/// <summary>
/// Command to triage a claim based on risk assessment.
/// Routing is deterministic and based on latest risk assessment.
/// </summary>
public class TriageClaimCommand
{
    public Guid ClaimId { get; set; }
}

/// <summary>
/// Command to override triage routing decision.
/// Requires human authorization and justification.
/// </summary>
public class OverrideTriageCommand
{
    public Guid ClaimId { get; set; }
    public string Queue { get; set; } = string.Empty;
    public string OverrideBy { get; set; } = string.Empty;
    public string OverrideReason { get; set; } = string.Empty;
}
