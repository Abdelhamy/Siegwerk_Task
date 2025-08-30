using Pricing.Application.Common.Interfaces;
using Pricing.Application.Interfaces;

namespace Pricing.Application.Services;

public interface ICurrencyService
{
    bool IsCurrencySupported(string currencyCode);
    decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency);
}
public class CurrencyService : ICurrencyService
{
    private readonly IRateProvider _rateProvider;
    private readonly IAppLogger<CurrencyService> _logger;

    public CurrencyService(IRateProvider rateProvider, IAppLogger<CurrencyService> logger)
    {
        _rateProvider = rateProvider;
        _logger = logger;
    }


    public bool IsCurrencySupported(string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            return false;

        try
        {
            var supportedCodes = _rateProvider.GetSupportedCurrencies();
            return supportedCodes.Contains(currencyCode, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if currency is supported: {CurrencyCode}", currencyCode);
            return false;
        }
    }

    public decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency)
    {
        try
        {
            _logger.LogDebug("Converting {Amount} from {FromCurrency} to {ToCurrency}", 
                amount, fromCurrency, toCurrency);

            var result = _rateProvider.Convert(amount, fromCurrency, toCurrency);

            _logger.LogDebug("Conversion result: {Amount} {FromCurrency} = {Result} {ToCurrency}", 
                amount, fromCurrency, result, toCurrency);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting currency: {Amount} {FromCurrency} to {ToCurrency}", 
                amount, fromCurrency, toCurrency);
            throw;
        }
    }
}