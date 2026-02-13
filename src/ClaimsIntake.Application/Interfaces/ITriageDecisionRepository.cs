// =============================================
// Repository Interface: ITriageDecisionRepository
// Description: Persistence for triage routing decisions
// Author: Application Team
// Date: February 2026
// =============================================

using ClaimsIntake.Domain.Entities;

namespace ClaimsIntake.Application.Interfaces;

/// <summary>
/// Repository for persisting triage routing decisions.
/// Triage decisions are immutable once created.
/// </summary>
public interface ITriageDecisionRepository
{
    Task<Guid> InsertAsync(TriageDecision decision, CancellationToken cancellationToken = default);
    
    Task<TriageDecision?> GetLatestByClaimIdAsync(Guid claimId, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<TriageDecision>> GetAllByClaimIdAsync(Guid claimId, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<TriageDecision>> GetByQueueAsync(string queue, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsForRiskAssessmentAsync(Guid claimId, Guid riskAssessmentId, CancellationToken cancellationToken = default);
}
