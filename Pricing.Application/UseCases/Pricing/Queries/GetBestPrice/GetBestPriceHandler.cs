using Pricing.Application.Common.Interfaces;
using Pricing.Application.Contracts;
using Pricing.Application.Interfaces;
using Pricing.Application.Interfaces.Repositories;
using Pricing.Domain.ValueObjects;

namespace Pricing.Application.UseCases.Pricing.Queries.GetBestPrice;

public class GetBestPriceHandler : IGetBestPriceHandler
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IRateProvider _rateProvider;
    private readonly IAppLogger<GetBestPriceHandler> _logger;

    public GetBestPriceHandler(
        ISupplierRepository supplierRepository,
        IRateProvider rateProvider,
        IAppLogger<GetBestPriceHandler> logger)
    {
        _supplierRepository = supplierRepository;
        _rateProvider = rateProvider;
        _logger = logger;
    }

    public async Task<GetBestPriceResponse> HandleAsync(
        GetBestPriceQuery query, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Handling best price query for SKU: {Sku}, Qty: {Quantity}, Currency: {Currency}, Date: {Date}",
                query.Sku, query.Qty, query.Currency, query.OnDate);

            var sku = Sku.Create(query.Sku);
            var quantity = Quantity.Create(query.Qty);
            
            var candidates = await _supplierRepository.GetValidPriceCandidatesAsync(sku, quantity, query.OnDate, null, null, cancellationToken);

            if (candidates.Count == 0)
            {
                _logger.LogInformation("No valid price candidates found for SKU: {Sku}, Qty: {Quantity}, Date: {Date}",
                    query.Sku, query.Qty, query.OnDate);
                return new GetBestPriceResponse(null);
            }

            _logger.LogDebug("Found {Count} price candidates for SKU: {Sku}", candidates.Count, query.Sku);

            var ranked = candidates
                .Select(c =>
                {
                    var unitInTarget = _rateProvider.Convert(c.PricePerUom, c.Currency, query.Currency);
                    return new
                    {
                        Candidate = c,
                        UnitPrice = unitInTarget,
                        TotalPrice = unitInTarget * query.Qty
                    };
                })
                .OrderBy(x => x.UnitPrice)                          
                .ThenByDescending(x => x.Candidate.SupplierPreferred) 
                .ThenBy(x => x.Candidate.SupplierLeadTimeDays)       
                .ThenBy(x => x.Candidate.SupplierId)                 
                .ToList();

            var best = ranked.First();

            _logger.LogInformation("Best price selected: Supplier {SupplierName} ({SupplierId}) - Unit: {UnitPrice} {Currency}, Total: {Total} {Currency}",
                best.Candidate.SupplierName, best.Candidate.SupplierId, best.UnitPrice, query.Currency, best.TotalPrice, query.Currency);

            var bestPriceResponse = new BestPriceResponse(
                Sku: query.Sku,
                Qty: query.Qty,
                Currency: query.Currency,
                UnitPrice: decimal.Round(best.UnitPrice, 4),
                Total: decimal.Round(best.TotalPrice, 2),
                SupplierId: best.Candidate.SupplierId,
                SupplierName: best.Candidate.SupplierName,
                SupplierPreferred: best.Candidate.SupplierPreferred,
                SupplierLeadTimeDays: best.Candidate.SupplierLeadTimeDays,
                Reason: "Lowest unit price (then Preferred, LeadTime, SupplierId)"
            );

            return new GetBestPriceResponse(bestPriceResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling best price query for SKU: {Sku}", query.Sku);
            throw;
        }
    }
}