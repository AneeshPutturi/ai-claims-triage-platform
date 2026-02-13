// =============================================
// Value Object: ClaimNumber
// Description: Business-meaningful claim identifier
// Author: Domain Team
// Date: February 2026
// =============================================
// Purpose: Encapsulates claim number validation and formatting.
// Immutable by design. Prevents invalid claim numbers from entering system.
// =============================================

namespace ClaimsIntake.Domain.ValueObjects;

/// <summary>
/// Business-meaningful claim identifier.
/// Format: YYYY-NNNNNN (e.g., 2026-000001)
/// Immutable once created.
/// </summary>
public sealed class ClaimNumber : IEquatable<ClaimNumber>
{
    public string Value { get; }

    private ClaimNumber(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Create a ClaimNumber from a string value.
    /// Validates format and throws if invalid.
    /// </summary>
    public static ClaimNumber From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Claim number cannot be empty", nameof(value));

        // Validate format: YYYY-NNNNNN
        if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^\d{4}-\d{6}$"))
            throw new ArgumentException(
                $"Claim number must be in format YYYY-NNNNNN. Got: {value}", 
                nameof(value));

        return new ClaimNumber(value);
    }

    /// <summary>
    /// Generate a new claim number for the current year.
    /// Sequence number must be provided by caller (from database sequence or counter).
    /// </summary>
    public static ClaimNumber Generate(int sequenceNumber)
    {
        if (sequenceNumber < 1 || sequenceNumber > 999999)
            throw new ArgumentException(
                "Sequence number must be between 1 and 999999", 
                nameof(sequenceNumber));

        var year = DateTime.UtcNow.Year;
        var value = $"{year}-{sequenceNumber:D6}";
        return new ClaimNumber(value);
    }

    public override string ToString() => Value;

    public bool Equals(ClaimNumber? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => Equals(obj as ClaimNumber);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(ClaimNumber? left, ClaimNumber? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(ClaimNumber? left, ClaimNumber? right) => !(left == right);
}
