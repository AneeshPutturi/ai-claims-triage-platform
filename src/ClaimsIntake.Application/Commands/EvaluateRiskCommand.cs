// =============================================
// Command: EvaluateRiskCommand
// Description: Trigger risk assessment for a claim
// Author: Application Team
// Date: February 2026
// =============================================

namespace ClaimsIntake.Application.Commands;

/// <summary>
/// Command to evaluate risk for a claim using verified data only.
/// Risk is a signal, not a verdict.
/// </summary>
public class EvaluateRiskCommand
{
    public Guid ClaimId { get; set; }
}
