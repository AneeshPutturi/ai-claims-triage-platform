// =============================================
// Repository Interface: IClaimDocumentRepository
// Description: Defines document metadata persistence operations
// Author: Application Team
// Date: February 2026
// =============================================

using ClaimsIntake.Domain.Entities;

namespace ClaimsIntake.Application.Interfaces;

/// <summary>
/// Repository for claim document metadata persistence.
/// File content is stored in blob storage, not database.
/// </summary>
public interface IClaimDocumentRepository
{
    /// <summary>
    /// Add document metadata after successful blob upload
    /// </summary>
    Task AddAsync(ClaimDocument document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve document metadata by ID
    /// </summary>
    Task<ClaimDocument?> GetByIdAsync(Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve all document metadata for a claim
    /// </summary>
    Task<IEnumerable<ClaimDocument>> GetByClaimIdAsync(Guid claimId, CancellationToken cancellationToken = default);
}
