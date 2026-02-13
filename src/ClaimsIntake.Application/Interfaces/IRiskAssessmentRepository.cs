// =============================================
// Repository Interface: IRiskAssessmentRepository
// Description: Persistence for risk assessment snapshots
// Author: Application Team
// Date: February 2026
// =============================================

using ClaimsIntake.Domain.Entities;

namespace ClaimsIntake.Application.Interfaces;

/// <summary>
/// Repository for persisting risk assessment snapshots.
/// Risk assessments are immutable once created.
/// </summary>
public interface IRiskAssessmentRepository
{
    Task<Guid> InsertAsync(RiskAssessment assessment, CancellationToken cancellationToken = default);
    
    Task<RiskAssessment?> GetLatestByClaimIdAsync(Guid claimId, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<RiskAssessment>> GetAllByClaimIdAsync(Guid claimId, CancellationToken cancellationToken = default);
    
    Task<RiskAssessment?> GetByIdAsync(Guid riskAssessmentId, CancellationToken cancellationToken = default);
}
