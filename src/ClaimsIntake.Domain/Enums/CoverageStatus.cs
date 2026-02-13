// =============================================
// Domain Enum: CoverageStatus
// Description: Policy coverage status on loss date
// Author: Domain Team
// Date: February 2026
// =============================================

namespace ClaimsIntake.Domain.Enums;

/// <summary>
/// Status of policy coverage on the loss date.
/// Determines whether claim is eligible for processing.
/// </summary>
public enum CoverageStatus
{
    /// <summary>
    /// Policy was active and provided coverage on loss date
    /// </summary>
    Active = 0,

    /// <summary>
    /// Policy had expired before loss date
    /// </summary>
    Expired = 1,

    /// <summary>
    /// Policy was cancelled before loss date
    /// </summary>
    Cancelled = 2,

    /// <summary>
    /// Policy was suspended on loss date
    /// </summary>
    Suspended = 3
}
