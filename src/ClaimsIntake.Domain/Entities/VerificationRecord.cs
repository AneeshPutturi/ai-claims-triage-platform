// =============================================
// Domain Entity: VerificationRecord
// Description: Human verification decision for AI-extracted data
// Author: Domain Team
// Date: February 2026
// =============================================

namespace ClaimsIntake.Domain.Entities;

/// <summary>
/// Represents a human verification decision on AI-extracted data.
/// Verification transfers accountability from system to human.
/// Immutable once created - verification decisions cannot be changed.
/// </summary>
public class VerificationRecord
{
    public Guid VerificationId { get; private set; }
    public Guid ClaimId { get; private set; }
    public Guid ExtractedFieldId { get; private set; }
    public string VerifiedBy { get; private set; }
    public DateTime VerifiedAt { get; private set; }
    public string ActionTaken { get; private set; }
    public string? CorrectedValue { get; private set; }
    public string? VerificationNotes { get; private set; }

    private VerificationRecord() { }

    public static VerificationRecord Create(
        Guid claimId,
        Guid extractedFieldId,
        string verifiedBy,
        string actionTaken,
        string? correctedValue = null,
        string? verificationNotes = null)
    {
        if (claimId == Guid.Empty)
            throw new ArgumentException("ClaimId cannot be empty", nameof(claimId));
        
        if (extractedFieldId == Guid.Empty)
            throw new ArgumentException("ExtractedFieldId cannot be empty", nameof(extractedFieldId));
        
        if (string.IsNullOrWhiteSpace(verifiedBy))
            throw new ArgumentException("VerifiedBy cannot be empty", nameof(verifiedBy));
        
        if (string.IsNullOrWhiteSpace(actionTaken))
            throw new ArgumentException("ActionTaken cannot be empty", nameof(actionTaken));
        
        // Validate action taken
        var validActions = new[] { "Accepted", "Corrected", "Rejected" };
        if (!validActions.Contains(actionTaken))
            throw new ArgumentException(
                $"ActionTaken must be one of: {string.Join(", ", validActions)}", 
                nameof(actionTaken));
        
        // If action is Corrected, correctedValue must be provided
        if (actionTaken == "Corrected" && string.IsNullOrWhiteSpace(correctedValue))
            throw new ArgumentException(
                "CorrectedValue is required when ActionTaken is Corrected", 
                nameof(correctedValue));

        return new VerificationRecord
        {
            VerificationId = Guid.NewGuid(),
            ClaimId = claimId,
            ExtractedFieldId = extractedFieldId,
            VerifiedBy = verifiedBy,
            VerifiedAt = DateTime.UtcNow,
            ActionTaken = actionTaken,
            CorrectedValue = correctedValue,
            VerificationNotes = verificationNotes
        };
    }
}
