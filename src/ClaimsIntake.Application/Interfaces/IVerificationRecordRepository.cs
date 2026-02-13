// =============================================
// Repository Interface: IVerificationRecordRepository
// Description: Persistence for human verification records
// Author: Application Team
// Date: February 2026
// =============================================

using ClaimsIntake.Domain.Entities;

namespace ClaimsIntake.Application.Interfaces;

/// <summary>
/// Repository for persisting human verification records.
/// Verification records are immutable once created.
/// </summary>
public interface IVerificationRecordRepository
{
    Task<Guid> InsertAsync(VerificationRecord record, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<VerificationRecord>> GetByClaimIdAsync(Guid claimId, CancellationToken cancellationToken = default);
    
    Task<VerificationRecord?> GetByExtractedFieldIdAsync(Guid extractedFieldId, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsForFieldAsync(Guid extractedFieldId, CancellationToken cancellationToken = default);
}
