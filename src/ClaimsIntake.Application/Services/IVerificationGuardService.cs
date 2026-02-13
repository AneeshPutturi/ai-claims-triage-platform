// =============================================
// Service Interface: IVerificationGuardService
// Description: Enforces verification requirements for downstream processing
// Author: Application Team
// Date: February 2026
// =============================================

using ClaimsIntake.Domain.Entities;

namespace ClaimsIntake.Application.Services;

/// <summary>
/// Guard service that prevents downstream use of unverified AI data.
/// Enforces the rule: AI output cannot be used until verified by a human.
/// </summary>
public interface IVerificationGuardService
{
    /// <summary>
    /// Validates that an extracted field has been verified.
    /// Throws exception if field is unverified.
    /// </summary>
    void EnsureVerified(ExtractedField field);

    /// <summary>
    /// Validates that all extracted fields for a claim have been verified.
    /// Throws exception if any field is unverified.
    /// </summary>
    Task EnsureAllVerifiedAsync(Guid claimId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets only verified extracted fields for a claim.
    /// Filters out unverified and rejected fields.
    /// </summary>
    Task<IEnumerable<ExtractedField>> GetVerifiedFieldsAsync(
        Guid claimId, 
        CancellationToken cancellationToken = default);
}
