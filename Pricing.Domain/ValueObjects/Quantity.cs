using Pricing.Domain.Common;

namespace Pricing.Domain.ValueObjects;

public sealed class Quantity : ValueObject
{
    public int Value { get; private set; }

    private Quantity() { } // EF Core constructor

    private Quantity(int value)
    {
        Value = value;
    }

    public static Quantity Create(int value)
    {
        if (value <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(value));

        return new Quantity(value);
    }

    public static implicit operator int(Quantity quantity) => quantity.Value;

    public static Quantity operator +(Quantity left, Quantity right) => new(left.Value + right.Value);

    public static Quantity operator -(Quantity left, Quantity right)
    {
        var result = left.Value - right.Value;
        if (result <= 0)
            throw new InvalidOperationException("Resulting quantity must be greater than zero");
        
        return new(result);
    }

    public static Quantity operator *(Quantity quantity, int multiplier)
    {
        if (multiplier <= 0)
            throw new ArgumentException("Multiplier must be greater than zero", nameof(multiplier));
        
        return new(quantity.Value * multiplier);
    }

    public static bool operator >(Quantity left, Quantity right) => left.Value > right.Value;
    public static bool operator <(Quantity left, Quantity right) => left.Value < right.Value;
    public static bool operator >=(Quantity left, Quantity right) => left.Value >= right.Value;
    public static bool operator <=(Quantity left, Quantity right) => left.Value <= right.Value;

    /// <summary>
    /// Checks if this quantity meets the minimum requirement
    /// </summary>
    public bool MeetsMinimum(Quantity minimumQuantity) => Value >= minimumQuantity.Value;

    /// <summary>
    /// Gets the quantity break tier for volume pricing
    /// </summary>
    public QuantityTier GetTier()
    {
        return Value switch
        {
            <= 10 => QuantityTier.Small,
            <= 100 => QuantityTier.Medium,
            <= 1000 => QuantityTier.Large,
            _ => QuantityTier.Bulk
        };
    }

    /// <summary>
    /// Validates that the requested quantity can be fulfilled by available price entries
    /// </summary>
    public bool CanBeFulfilledBy(IEnumerable<Quantity> availableMinimums)
    {
        return availableMinimums?.Any(min => Value >= min.Value) ?? false;
    }

    /// <summary>
    /// Gets the percentage above minimum for pricing calculations
    /// </summary>
    public decimal GetPercentageAboveMinimum(Quantity minimum)
    {
        if (Value < minimum.Value)
            return 0;

        return minimum.Value == 0 ? 0 : ((decimal)(Value - minimum.Value) / minimum.Value) * 100;
    }

    public override string ToString() => Value.ToString();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}

public enum QuantityTier
{
    Small,
    Medium,
    Large,
    Bulk
}