using Pricing.Application.Common.Interfaces;
using Pricing.Application.Common.Models;
using Pricing.Application.Contracts;
using Pricing.Application.Interfaces.Repositories;
using Pricing.Domain.ValueObjects;

namespace Pricing.Application.UseCases.PriceLists.Queries.GetPricesPagedQuery;

public class GetPricesPagedHandler : IGetPricesPagedHandler
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IAppLogger<GetPricesPagedHandler> _logger;

    public GetPricesPagedHandler(
        ISupplierRepository supplierRepository,
        IAppLogger<GetPricesPagedHandler> logger)
    {
        _supplierRepository = supplierRepository;
        _logger = logger;
    }

    public async Task<GetPricesPagedResponse> HandleAsync(
        GetPricesPagedQuery query, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting paged prices with filters - SKU: {Sku}, Quantity: {Quantity}, ValidOn: {ValidOn}, Currency: {Currency}, SupplierId: {SupplierId}", 
                query.Sku, query.Quantity, query.ValidOn, query.Currency, query.SupplierId);

            var sku = !string.IsNullOrEmpty(query.Sku) ? Sku.Create(query.Sku) : null;
            var quantity = query.Quantity.HasValue ? Quantity.Create(query.Quantity.Value) : null;

            var candidates = await _supplierRepository.GetValidPriceCandidatesAsync(
                sku, 
                quantity, 
                query.ValidOn, 
                query.Currency, 
                query.SupplierId, 
                cancellationToken);

            var filteredCandidates = candidates.AsQueryable();

            filteredCandidates = ApplySort(filteredCandidates, query.Sort);

            var totalCount = filteredCandidates.Count();

            // Apply pagination
            var normalizedPagination = query.Pagination.Normalize();
            var pagedItems = filteredCandidates
                .Skip(normalizedPagination.Skip)
                .Take(normalizedPagination.Take)
                .ToList();

            var result = PagedResult<PriceCandidateDto>.Create(
                pagedItems,
                totalCount,
                normalizedPagination.Page,
                normalizedPagination.PageSize);

            _logger.LogInformation("Retrieved {Count} prices out of {TotalCount} total", 
                result.Count, result.TotalCount);

            return new GetPricesPagedResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged prices");
            throw;
        }
    }

    private static IQueryable<PriceCandidateDto> ApplySort(IQueryable<PriceCandidateDto> query, SortRequest? sort)
    {
        if (sort == null || string.IsNullOrWhiteSpace(sort.SortBy))
            return query.OrderBy(p => p.Sku).ThenBy(p => p.SupplierName);

        return sort.SortBy.ToLowerInvariant() switch
        {
            "sku" => sort.IsDescending ? query.OrderByDescending(p => p.Sku) : query.OrderBy(p => p.Sku),
            "suppliername" => sort.IsDescending ? query.OrderByDescending(p => p.SupplierName) : query.OrderBy(p => p.SupplierName),
            "supplierid" => sort.IsDescending ? query.OrderByDescending(p => p.SupplierId) : query.OrderBy(p => p.SupplierId),
            "price" or "pricepeuom" => sort.IsDescending ? query.OrderByDescending(p => p.PricePerUom) : query.OrderBy(p => p.PricePerUom),
            "currency" => sort.IsDescending ? query.OrderByDescending(p => p.Currency) : query.OrderBy(p => p.Currency),
            "validfrom" => sort.IsDescending ? query.OrderByDescending(p => p.ValidFrom) : query.OrderBy(p => p.ValidFrom),
            "validto" => sort.IsDescending ? query.OrderByDescending(p => p.ValidTo) : query.OrderBy(p => p.ValidTo),
            "minqty" => sort.IsDescending ? query.OrderByDescending(p => p.MinQty) : query.OrderBy(p => p.MinQty),
            "preferred" => sort.IsDescending ? query.OrderByDescending(p => p.SupplierPreferred) : query.OrderBy(p => p.SupplierPreferred),
            "leadtime" => sort.IsDescending ? query.OrderByDescending(p => p.SupplierLeadTimeDays) : query.OrderBy(p => p.SupplierLeadTimeDays),
            _ => query.OrderBy(p => p.Sku).ThenBy(p => p.SupplierName)
        };
    }
}