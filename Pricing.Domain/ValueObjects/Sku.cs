using Pricing.Domain.Common;

namespace Pricing.Domain.ValueObjects;

public sealed class Sku : ValueObject
{
    public string Value { get; private set; } = string.Empty;

    private Sku() { } // EF Core constructor

    private Sku(string value)
    {
        Value = value;
    }

    public static Sku Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SKU cannot be null or empty", nameof(value));

        if (value.Length > 50)
            throw new ArgumentException("SKU cannot exceed 50 characters", nameof(value));

        var normalizedValue = value.Trim().ToUpperInvariant();
        
        if (!IsValidSku(normalizedValue))
            throw new ArgumentException($"Invalid SKU format: {value}", nameof(value));

        return new Sku(normalizedValue);
    }

    private static bool IsValidSku(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (!char.IsLetterOrDigit(value[0]))
            return false;

        return value.All(c => char.IsLetterOrDigit(c) || c == '-');
    }

    public static implicit operator string(Sku sku) => sku.Value;

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}