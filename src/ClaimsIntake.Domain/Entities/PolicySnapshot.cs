// =============================================
// Domain Entity: PolicySnapshot
// Description: Point-in-time policy coverage record
// Author: Domain Team
// Date: February 2026
// =============================================

using ClaimsIntake.Domain.Enums;
using ClaimsIntake.Domain.ValueObjects;

namespace ClaimsIntake.Domain.Entities;

/// <summary>
/// Point-in-time record of policy coverage status as of loss date.
/// Immutable once created - never updated.
/// </summary>
public class PolicySnapshot
{
    public Guid SnapshotId { get; private set; }
    public Guid ClaimId { get; private set; }
    public PolicyId PolicyId { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    public DateTime ExpirationDate { get; private set; }
    public CoverageStatus CoverageStatus { get; private set; }
    public string CoveredLossTypes { get; private set; } // JSON array
    public string? CoverageLimits { get; private set; } // JSON object
    public string? Deductibles { get; private set; } // JSON object
    public DateTime SnapshotCreatedAt { get; private set; }

    private PolicySnapshot() { }

    public static PolicySnapshot Create(
        Guid claimId,
        PolicyId policyId,
        DateTime effectiveDate,
        DateTime expirationDate,
        CoverageStatus coverageStatus,
        string coveredLossTypes,
        string? coverageLimits = null,
        string? deductibles = null)
    {
        if (effectiveDate >= expirationDate)
            throw new ArgumentException("Effective date must be before expiration date");

        if (string.IsNullOrWhiteSpace(coveredLossTypes))
            throw new ArgumentException("Covered loss types are required", nameof(coveredLossTypes));

        return new PolicySnapshot
        {
            SnapshotId = Guid.NewGuid(),
            ClaimId = claimId,
            PolicyId = policyId,
            EffectiveDate = effectiveDate.Date,
            ExpirationDate = expirationDate.Date,
            CoverageStatus = coverageStatus,
            CoveredLossTypes = coveredLossTypes,
            CoverageLimits = coverageLimits,
            Deductibles = deductibles,
            SnapshotCreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Determine if policy was in force on a specific date
    /// </summary>
    public bool WasInForceOn(DateTime date)
    {
        return CoverageStatus == CoverageStatus.Active
            && date.Date >= EffectiveDate.Date
            && date.Date <= ExpirationDate.Date;
    }
}
