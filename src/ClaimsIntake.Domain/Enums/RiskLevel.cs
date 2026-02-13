// =============================================
// Domain Enum: RiskLevel
// Description: Risk assessment levels for claim routing
// Author: Domain Team
// Date: February 2026
// =============================================

namespace ClaimsIntake.Domain.Enums;

/// <summary>
/// Risk level assigned to claim for routing purposes.
/// Risk is a routing signal, not a fraud verdict or coverage determination.
/// </summary>
public enum RiskLevel
{
    /// <summary>
    /// Low risk - route to standard processing queue
    /// </summary>
    Low = 0,

    /// <summary>
    /// Medium risk - route to standard processing with additional review
    /// </summary>
    Medium = 1,

    /// <summary>
    /// High risk - route to specialized investigation queue
    /// </summary>
    High = 2,

    /// <summary>
    /// Critical risk - route to senior adjuster for immediate review
    /// </summary>
    Critical = 3
}
