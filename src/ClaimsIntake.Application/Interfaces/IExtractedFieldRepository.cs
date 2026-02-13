// =============================================
// Repository Interface: IExtractedFieldRepository
// Description: Persistence for AI-extracted fields
// Author: Application Team
// Date: February 2026
// =============================================

using ClaimsIntake.Domain.Entities;

namespace ClaimsIntake.Application.Interfaces;

/// <summary>
/// Repository for persisting AI-extracted fields.
/// </summary>
public interface IExtractedFieldRepository
{
    Task<Guid> InsertAsync(ExtractedField field, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<ExtractedField>> GetByClaimIdAsync(Guid claimId, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<ExtractedField>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
    
    Task<ExtractedField?> GetByIdAsync(Guid extractedFieldId, CancellationToken cancellationToken = default);
    
    Task UpdateVerificationStatusAsync(Guid extractedFieldId, string verificationStatus, CancellationToken cancellationToken = default);
}
