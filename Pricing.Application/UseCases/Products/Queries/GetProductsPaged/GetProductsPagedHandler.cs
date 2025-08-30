using Pricing.Application.Common.Interfaces;
using Pricing.Application.Interfaces.Repositories;

namespace Pricing.Application.UseCases.Products.Queries.GetProductsPaged;

public class GetProductsPagedHandler : IGetProductsPagedHandler
{
    private readonly IProductRepository _productRepository;
    private readonly IAppLogger<GetProductsPagedHandler> _logger;

    public GetProductsPagedHandler(
        IProductRepository productRepository,
        IAppLogger<GetProductsPagedHandler> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<GetProductsPagedResponse> HandleAsync(
        GetProductsPagedQuery query, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting paged products with pagination: {Pagination}, sort: {Sort}", 
                query.Pagination, query.Sort);

            var result = await _productRepository.GetPagedAsync(
                query.Pagination,
                query.Sort,
                query.SearchTerm,
                query.Name,
                query.Sku,
                query.UnitOfMeasure,
                query.HazardClass,
                query.IsHazardous,
                cancellationToken);

            _logger.LogInformation("Retrieved {Count} products out of {TotalCount} total", 
                result.Count, result.TotalCount);

            return new GetProductsPagedResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged products");
            throw;
        }
    }
}