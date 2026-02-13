// =============================================
// Service Implementation: VerificationGuardService
// Description: Enforces verification requirements
// Author: Infrastructure Team
// Date: February 2026
// =============================================

using ClaimsIntake.Application.Interfaces;
using ClaimsIntake.Application.Services;
using ClaimsIntake.Domain.Entities;
using ClaimsIntake.Domain.Enums;

namespace ClaimsIntake.Infrastructure.Services;

/// <summary>
/// Guard service that prevents downstream use of unverified AI data.
/// Fails loudly when unverified data is accessed.
/// </summary>
public class VerificationGuardService : IVerificationGuardService
{
    private readonly IExtractedFieldRepository _extractedFieldRepository;

    public VerificationGuardService(IExtractedFieldRepository extractedFieldRepository)
    {
        _extractedFieldRepository = extractedFieldRepository;
    }

    public void EnsureVerified(ExtractedField field)
    {
        if (field.VerificationStatus == VerificationStatus.Unverified)
        {
            throw new InvalidOperationException(
                $"Cannot use unverified AI data. Field '{field.FieldName}' (ID: {field.ExtractedFieldId}) " +
                "must be verified by a human before it can be used for downstream processing. " +
                "AI output is data, not truth. Human verification is required.");
        }

        if (field.VerificationStatus == VerificationStatus.Rejected)
        {
            throw new InvalidOperationException(
                $"Cannot use rejected AI data. Field '{field.FieldName}' (ID: {field.ExtractedFieldId}) " +
                "was rejected during human verification and should not be used for downstream processing.");
        }
    }

    public async Task EnsureAllVerifiedAsync(Guid claimId, CancellationToken cancellationToken = default)
    {
        var fields = await _extractedFieldRepository.GetByClaimIdAsync(claimId, cancellationToken);

        var unverifiedFields = fields
            .Where(f => f.VerificationStatus == VerificationStatus.Unverified)
            .ToList();

        if (unverifiedFields.Any())
        {
            var fieldNames = string.Join(", ", unverifiedFields.Select(f => f.FieldName));
            throw new InvalidOperationException(
                $"Cannot process claim {claimId}. The following fields are unverified: {fieldNames}. " +
                "All AI-extracted data must be verified by a human before downstream processing. " +
                "AI output is data, not truth. Human verification is required.");
        }
    }

    public async Task<IEnumerable<ExtractedField>> GetVerifiedFieldsAsync(
        Guid claimId,
        CancellationToken cancellationToken = default)
    {
        var fields = await _extractedFieldRepository.GetByClaimIdAsync(claimId, cancellationToken);

        // Return only verified or corrected fields (exclude unverified and rejected)
        return fields.Where(f =>
            f.VerificationStatus == VerificationStatus.Verified ||
            f.VerificationStatus == VerificationStatus.Corrected);
    }
}
