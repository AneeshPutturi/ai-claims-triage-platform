// =============================================
// Application Command: SubmitClaimCommand
// Description: Command for submitting First Notice of Loss
// Author: Application Team
// Date: February 2026
// =============================================
// Purpose: Contains only necessary data for claim submission.
// No infrastructure dependencies, no HTTP concerns.
// =============================================

namespace ClaimsIntake.Application.Commands;

/// <summary>
/// Command for submitting a First Notice of Loss (FNOL).
/// Represents the minimal but accurate data required to initiate claim processing.
/// </summary>
public record SubmitClaimCommand(
    string PolicyNumber,
    DateTime LossDate,
    string LossType,
    string LossLocation,
    string LossDescription,
    string SubmittedBy)
{
    /// <summary>
    /// Validate command data before processing
    /// </summary>
    public void Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(PolicyNumber))
            errors.Add("Policy number is required");

        if (LossDate == default)
            errors.Add("Loss date is required");

        if (string.IsNullOrWhiteSpace(LossType))
            errors.Add("Loss type is required");

        if (string.IsNullOrWhiteSpace(LossLocation))
            errors.Add("Loss location is required");

        if (string.IsNullOrWhiteSpace(SubmittedBy))
            errors.Add("Submitter identity is required");

        if (errors.Any())
            throw new ArgumentException($"Validation failed: {string.Join(", ", errors)}");
    }
}
