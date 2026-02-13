// =============================================
// Service Interface: IPolicyValidationService
// Description: Policy validation and snapshot creation
// Author: Application Team
// Date: February 2026
// =============================================

using ClaimsIntake.Domain.Entities;
using ClaimsIntake.Domain.ValueObjects;

namespace ClaimsIntake.Application.Services;

/// <summary>
/// Service for validating policy coverage and creating snapshots.
/// Queries external policy system and captures point-in-time coverage.
/// </summary>
public interface IPolicyValidationService
{
    /// <summary>
    /// Validate policy coverage on loss date and create snapshot.
    /// Throws exception if policy not found or coverage invalid.
    /// </summary>
    Task<PolicySnapshot> ValidateAndCreateSnapshotAsync(
        Guid claimId,
        PolicyId policyId,
        LossDate lossDate,
        CancellationToken cancellationToken = default);
}
