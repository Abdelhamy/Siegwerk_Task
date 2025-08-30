using Pricing.Domain.Common;

namespace Pricing.Domain.ValueObjects;

public sealed class Currency : ValueObject
{
    public string Code { get; private set; } = string.Empty;

    private Currency() { } // EF Core constructor

    private Currency(string code)
    {
        Code = code;
    }

    public static Currency Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Currency code cannot be null or empty", nameof(code));

        if (code.Length != 3)
            throw new ArgumentException("Currency code must be exactly 3 characters", nameof(code));

        var normalizedCode = code.Trim().ToUpperInvariant();
        
        if (!IsValidCurrency(normalizedCode))
            throw new ArgumentException($"Invalid currency code: {code}", nameof(code));

        return new Currency(normalizedCode);
    }

    private static bool IsValidCurrency(string code)
    {
        var supportedCurrencies = new[] { "EUR", "USD", "EGP" };
        return supportedCurrencies.Contains(code);
    }

    public static readonly Currency EUR = new("EUR");
    public static readonly Currency USD = new("USD");
    public static readonly Currency EGP = new("EGP");

    public static implicit operator string(Currency currency) => currency.Code;

    public override string ToString() => Code;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Code;
    }

    public static Currency FromCode(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency code cannot be null or empty", nameof(currency));

        return currency.Trim().ToUpperInvariant() switch
        {
            "EUR" => EUR,
            "USD" => USD,
            "EGP" => EGP,
            _ => throw new ArgumentException($"Invalid currency code: {currency}", nameof(currency))
        };
    }
}