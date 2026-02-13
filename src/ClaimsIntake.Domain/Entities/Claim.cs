// =============================================
// Domain Entity: Claim
// Description: Represents a time-bound loss event request tied to a policy
// Author: Domain Team
// Date: February 2026
// =============================================
// Purpose: Pure domain entity with no persistence concerns.
// Expresses business meaning and enforces invariants.
// No EF attributes, no Azure references.
// =============================================

using ClaimsIntake.Domain.Enums;
using ClaimsIntake.Domain.ValueObjects;

namespace ClaimsIntake.Domain.Entities;

/// <summary>
/// A Claim represents a formal request for insurance coverage related to a specific,
/// time-bound loss event. It is always tied to a specific policy and a specific loss date.
/// </summary>
public class Claim
{
    /// <summary>
    /// System-generated unique identifier (surrogate key)
    /// </summary>
    public Guid ClaimId { get; private set; }

    /// <summary>
    /// Business-meaningful identifier (human-readable)
    /// Immutable once assigned
    /// </summary>
    public ClaimNumber ClaimNumber { get; private set; }

    /// <summary>
    /// Policy reference provided by claimant
    /// Immutable once assigned
    /// </summary>
    public PolicyId PolicyNumber { get; private set; }

    /// <summary>
    /// Date the loss occurred as reported by claimant
    /// Immutable once assigned
    /// </summary>
    public LossDate LossDate { get; private set; }

    /// <summary>
    /// Type of loss (e.g., PropertyDamage, Liability, BusinessInterruption)
    /// Immutable once assigned
    /// </summary>
    public string LossType { get; private set; }

    /// <summary>
    /// Location where the loss occurred
    /// Immutable once assigned
    /// </summary>
    public string LossLocation { get; private set; }

    /// <summary>
    /// Textual description of the loss
    /// May be updated during verification
    /// </summary>
    public string LossDescription { get; private set; }

    /// <summary>
    /// Current lifecycle state
    /// Only column that changes as claim progresses
    /// </summary>
    public ClaimStatus Status { get; private set; }

    /// <summary>
    /// Creation timestamp
    /// Immutable - establishes legal trigger
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Last update timestamp
    /// Updated automatically on any change
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Submitter identity
    /// Immutable - establishes accountability
    /// </summary>
    public string SubmittedBy { get; private set; }

    /// <summary>
    /// Concurrency control token
    /// </summary>
    public byte[] RowVersion { get; private set; }

    // Private constructor for EF/persistence
    private Claim() { }

    /// <summary>
    /// Factory method for creating a new claim
    /// Enforces invariants at creation time
    /// </summary>
    public static Claim Create(
        ClaimNumber claimNumber,
        PolicyId policyNumber,
        LossDate lossDate,
        string lossType,
        string lossLocation,
        string lossDescription,
        string submittedBy)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(lossType))
            throw new ArgumentException("Loss type is required", nameof(lossType));

        if (string.IsNullOrWhiteSpace(lossLocation))
            throw new ArgumentException("Loss location is required", nameof(lossLocation));

        if (string.IsNullOrWhiteSpace(submittedBy))
            throw new ArgumentException("Submitter identity is required", nameof(submittedBy));

        var now = DateTime.UtcNow;

        return new Claim
        {
            ClaimId = Guid.NewGuid(),
            ClaimNumber = claimNumber,
            PolicyNumber = policyNumber,
            LossDate = lossDate,
            LossType = lossType,
            LossLocation = lossLocation,
            LossDescription = lossDescription,
            Status = ClaimStatus.Submitted,
            CreatedAt = now,
            UpdatedAt = now,
            SubmittedBy = submittedBy
        };
    }

    /// <summary>
    /// Transition claim to Validated status
    /// Enforces valid state transitions
    /// </summary>
    public void MarkAsValidated()
    {
        if (Status != ClaimStatus.Submitted)
            throw new InvalidOperationException(
                $"Cannot transition from {Status} to Validated. Claim must be in Submitted state.");

        Status = ClaimStatus.Validated;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Transition claim to Verified status
    /// Enforces valid state transitions
    /// </summary>
    public void MarkAsVerified()
    {
        if (Status != ClaimStatus.Validated)
            throw new InvalidOperationException(
                $"Cannot transition from {Status} to Verified. Claim must be in Validated state.");

        Status = ClaimStatus.Verified;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Transition claim to Triaged status
    /// Enforces valid state transitions
    /// </summary>
    public void MarkAsTriaged()
    {
        if (Status != ClaimStatus.Verified)
            throw new InvalidOperationException(
                $"Cannot transition from {Status} to Triaged. Claim must be in Verified state.");

        Status = ClaimStatus.Triaged;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update loss description during verification
    /// Only allowed in Validated or Verified states
    /// </summary>
    public void UpdateLossDescription(string newDescription)
    {
        if (Status != ClaimStatus.Validated && Status != ClaimStatus.Verified)
            throw new InvalidOperationException(
                $"Cannot update description in {Status} state. Claim must be Validated or Verified.");

        if (string.IsNullOrWhiteSpace(newDescription))
            throw new ArgumentException("Loss description cannot be empty", nameof(newDescription));

        LossDescription = newDescription;
        UpdatedAt = DateTime.UtcNow;
    }
}
