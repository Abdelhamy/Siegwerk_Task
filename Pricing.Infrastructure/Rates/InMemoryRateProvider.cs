using Pricing.Application.Interfaces;

namespace Pricing.Infrastructure.Rates;

public class InMemoryRateProvider : IRateProvider
{
    private static readonly IReadOnlyDictionary<string, decimal> ExchangeRates =
        new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["EUR"] = 1.00m,
            ["USD"] = 1.09m,   // 1 EUR = 1.09 USD
            ["EGP"] = 54.35m   // 1 EUR = 54.35 EGP
        };
    public decimal Convert(decimal amount, string from, string to)
    {
        ValidateInputs(amount, from, to);

        if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase))
            return amount;

        var fromRate = GetExchangeRate(from);
        var toRate = GetExchangeRate(to);

        // Convert: amount (from currency) -> EUR -> target currency
        var amountInEur = amount / fromRate;
        var convertedAmount = amountInEur * toRate;

        return Math.Round(convertedAmount, 4, MidpointRounding.AwayFromZero);
    }

    public IEnumerable<string> GetSupportedCurrencies()
    {
        return ExchangeRates.Keys;
    }

    private static void ValidateInputs(decimal amount, string from, string to)
    {
        if (string.IsNullOrWhiteSpace(from))
            throw new ArgumentException("Source currency cannot be null, empty, or whitespace.", nameof(from));

        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("Target currency cannot be null, empty, or whitespace.", nameof(to));

        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
    }

    private static decimal GetExchangeRate(string currencyCode)
    {
        if (!ExchangeRates.TryGetValue(currencyCode, out var rate))
        {
            var supportedCurrencies = string.Join(", ", ExchangeRates.Keys);
            throw new InvalidOperationException(
                $"Unsupported currency: '{currencyCode}'. Supported currencies: {supportedCurrencies}");
        }

        return rate;
    }
}
