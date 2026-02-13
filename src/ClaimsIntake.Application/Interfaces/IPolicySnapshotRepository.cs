// =============================================
// Repository Interface: IPolicySnapshotRepository
// Description: Defines policy snapshot persistence operations
// Author: Application Team
// Date: February 2026
// =============================================

using ClaimsIntake.Domain.Entities;

namespace ClaimsIntake.Application.Interfaces;

/// <summary>
/// Repository for policy snapshot persistence.
/// Snapshots are immutable once created.
/// </summary>
public interface IPolicySnapshotRepository
{
    /// <summary>
    /// Add policy snapshot for claim
    /// </summary>
    Task AddAsync(PolicySnapshot snapshot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve policy snapshot by claim ID
    /// </summary>
    Task<PolicySnapshot?> GetByClaimIdAsync(Guid claimId, CancellationToken cancellationToken = default);
}
