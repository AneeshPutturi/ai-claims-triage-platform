// =============================================
// Value Object: LossDate
// Description: Date the loss occurred
// Author: Domain Team
// Date: February 2026
// =============================================

namespace ClaimsIntake.Domain.ValueObjects;

/// <summary>
/// Date the loss occurred as reported by claimant.
/// Must be a specific calendar date, not a range or estimate.
/// Immutable once created.
/// </summary>
public sealed class LossDate : IEquatable<LossDate>
{
    public DateTime Value { get; }

    private LossDate(DateTime value)
    {
        Value = value;
    }

    public static LossDate From(DateTime value)
    {
        // Loss date must be in the past or today
        if (value.Date > DateTime.UtcNow.Date)
            throw new ArgumentException(
                "Loss date cannot be in the future", 
                nameof(value));

        // Loss date cannot be more than 10 years in the past (business rule)
        var tenYearsAgo = DateTime.UtcNow.AddYears(-10);
        if (value.Date < tenYearsAgo.Date)
            throw new ArgumentException(
                "Loss date cannot be more than 10 years in the past", 
                nameof(value));

        // Store as date only (no time component)
        return new LossDate(value.Date);
    }

    public override string ToString() => Value.ToString("yyyy-MM-dd");

    public bool Equals(LossDate? other)
    {
        if (other is null) return false;
        return Value.Date == other.Value.Date;
    }

    public override bool Equals(object? obj) => Equals(obj as LossDate);

    public override int GetHashCode() => Value.Date.GetHashCode();

    public static bool operator ==(LossDate? left, LossDate? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(LossDate? left, LossDate? right) => !(left == right);
}
