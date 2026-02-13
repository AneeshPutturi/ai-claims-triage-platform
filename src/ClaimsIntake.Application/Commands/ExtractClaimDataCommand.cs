// =============================================
// Command: ExtractClaimDataCommand
// Description: Trigger AI extraction for a claim document
// Author: Application Team
// Date: February 2026
// =============================================

namespace ClaimsIntake.Application.Commands;

/// <summary>
/// Command to extract structured data from a claim document using AI.
/// Results are stored as unverified by default.
/// </summary>
public class ExtractClaimDataCommand
{
    public Guid ClaimId { get; set; }
    public Guid DocumentId { get; set; }
    public string Actor { get; set; } = string.Empty;
}
