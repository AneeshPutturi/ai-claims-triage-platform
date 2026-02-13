// =============================================
// Value Object: PolicyId
// Description: External policy identifier
// Author: Domain Team
// Date: February 2026
// =============================================

namespace ClaimsIntake.Domain.ValueObjects;

/// <summary>
/// External policy identifier from policy system.
/// Immutable once created.
/// </summary>
public sealed class PolicyId : IEquatable<PolicyId>
{
    public string Value { get; }

    private PolicyId(string value)
    {
        Value = value;
    }

    public static PolicyId From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Policy ID cannot be empty", nameof(value));

        if (value.Length > 100)
            throw new ArgumentException("Policy ID cannot exceed 100 characters", nameof(value));

        return new PolicyId(value.Trim());
    }

    public override string ToString() => Value;

    public bool Equals(PolicyId? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => Equals(obj as PolicyId);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(PolicyId? left, PolicyId? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(PolicyId? left, PolicyId? right) => !(left == right);
}
