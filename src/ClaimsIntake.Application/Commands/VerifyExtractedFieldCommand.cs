// =============================================
// Command: VerifyExtractedFieldCommand
// Description: Human verification decision for AI-extracted field
// Author: Application Team
// Date: February 2026
// =============================================

namespace ClaimsIntake.Application.Commands;

/// <summary>
/// Command representing a human verification decision.
/// Verification transfers accountability from system to human.
/// </summary>
public class VerifyExtractedFieldCommand
{
    public Guid ExtractedFieldId { get; set; }
    public string VerifiedBy { get; set; } = string.Empty;
    public string ActionTaken { get; set; } = string.Empty;
    public string? CorrectedValue { get; set; }
    public string? VerificationNotes { get; set; }
}
