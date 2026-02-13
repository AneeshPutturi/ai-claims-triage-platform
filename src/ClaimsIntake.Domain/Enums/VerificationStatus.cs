// =============================================
// Domain Enum: VerificationStatus
// Description: Status of AI-extracted field verification
// Author: Domain Team
// Date: February 2026
// =============================================

namespace ClaimsIntake.Domain.Enums;

/// <summary>
/// Verification status for AI-extracted fields.
/// All extracted data is Unverified by default until human review.
/// </summary>
public enum VerificationStatus
{
    /// <summary>
    /// AI-extracted data has not been reviewed by human.
    /// Cannot be used for downstream processing.
    /// </summary>
    Unverified = 0,

    /// <summary>
    /// Human adjuster confirmed AI extraction is accurate.
    /// Data can be trusted for downstream processing.
    /// </summary>
    Verified = 1,

    /// <summary>
    /// Human adjuster corrected AI extraction.
    /// Corrected value is stored in VerificationRecords.
    /// </summary>
    Corrected = 2,

    /// <summary>
    /// Human adjuster determined AI extraction is incorrect.
    /// Data should not be used.
    /// </summary>
    Rejected = 3
}
