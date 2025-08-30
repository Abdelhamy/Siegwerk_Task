namespace Pricing.Application.Interfaces;

public interface IRateProvider
{

    decimal Convert(decimal amount, string from, string to);

    IEnumerable<string> GetSupportedCurrencies();
}