using Pricing.Domain.Common;

namespace Pricing.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public Currency Currency { get; private set; }

    private Money() { } // EF Core constructor

    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, Currency currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        if (currency is null)
            throw new ArgumentNullException(nameof(currency));

        return new Money(amount, currency);
    }

    public Money ConvertTo(Currency targetCurrency, decimal exchangeRate)
    {
        if (Currency == targetCurrency)
            return this;

        var convertedAmount = Amount * exchangeRate;
        return new Money(Math.Round(convertedAmount, 4, MidpointRounding.AwayFromZero), targetCurrency);
    }

    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");

        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies");

        return new Money(left.Amount - right.Amount, left.Currency);
    }

    public static Money operator *(Money money, decimal multiplier)
    {
        return new Money(money.Amount * multiplier, money.Currency);
    }

    public static Money operator *(decimal multiplier, Money money)
    {
        return money * multiplier;
    }

    public static bool operator >(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot compare money with different currencies");

        return left.Amount > right.Amount;
    }

    public static bool operator <(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot compare money with different currencies");

        return left.Amount < right.Amount;
    }

    public static bool operator >=(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot compare money with different currencies");

        return left.Amount >= right.Amount;
    }

    public static bool operator <=(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot compare money with different currencies");

        return left.Amount <= right.Amount;
    }

    public bool IsZero => Amount == 0;

    public bool IsPositive => Amount > 0;

    public bool IsNegative => Amount < 0;

    public Money Abs() => new(Math.Abs(Amount), Currency);

    public override string ToString() => $"{Amount:F4} {Currency}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}