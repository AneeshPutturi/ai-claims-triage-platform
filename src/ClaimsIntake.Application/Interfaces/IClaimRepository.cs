// =============================================
// Repository Interface: IClaimRepository
// Description: Defines claim persistence operations
// Author: Application Team
// Date: February 2026
// =============================================
// Purpose: Express intent without implementation details.
// No SQL, no EF, no infrastructure concerns.
// =============================================

using ClaimsIntake.Domain.Entities;
using ClaimsIntake.Domain.ValueObjects;

namespace ClaimsIntake.Application.Interfaces;

/// <summary>
/// Repository for claim persistence operations.
/// Implementation will use explicit SQL with managed identity.
/// </summary>
public interface IClaimRepository
{
    /// <summary>
    /// Retrieve claim by system-generated ID
    /// </summary>
    Task<Claim?> GetByIdAsync(Guid claimId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve claim by business-meaningful claim number
    /// </summary>
    Task<Claim?> GetByClaimNumberAsync(ClaimNumber claimNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add new claim to system
    /// </summary>
    Task AddAsync(Claim claim, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing claim
    /// Throws concurrency exception if RowVersion has changed
    /// </summary>
    Task UpdateAsync(Claim claim, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get next sequence number for claim number generation
    /// </summary>
    Task<int> GetNextSequenceNumberAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update claim status (for state transitions)
    /// </summary>
    Task UpdateStatusAsync(Guid claimId, string status, CancellationToken cancellationToken = default);
}
